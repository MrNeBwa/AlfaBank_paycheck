using AlfaBank.Core.Data;
using AlfaBank.Core.Models;
using AlfaBank.Core.Models.Enums;
using AlfaBank.Core.Results;
using AlfaBank.Core.Services.Contracts;
using AlfaBank.Core.Services.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AlfaBank.Core.Services;

/// <summary>
/// Сервис кредитного специалиста: проверка заявок, скоринг, формирование кредита и учёт платежей.
/// </summary>
public sealed class CreditSpecialistService : ICreditSpecialistService
{
    private const string ApplicationNotFoundMessage = "Заявка не найдена.";
    private const string ApplicationAlreadyProcessedMessage = "Заявка уже обработана другим специалистом.";
    private const string RejectionCommentRequiredMessage = "Укажите причину отказа.";
    private const string CreditNotFoundMessage = "Кредит не найден.";

    private readonly Func<AppDbContext> _contextFactory;
    private readonly ICreditCalculator _creditCalculator;
    private readonly IScoringService _scoringService;

    /// <summary>
    /// Создаёт сервис кредитного специалиста.
    /// </summary>
    /// <param name="contextFactory">Фабрика контекстов базы данных.</param>
    /// <param name="creditCalculator">Кредитный калькулятор.</param>
    /// <param name="scoringService">Сервис скоринга заёмщика.</param>
    public CreditSpecialistService(
        Func<AppDbContext> contextFactory,
        ICreditCalculator creditCalculator,
        IScoringService scoringService)
    {
        _contextFactory = contextFactory;
        _creditCalculator = creditCalculator;
        _scoringService = scoringService;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ApplicationSummaryDto>> GetQueueAsync(
        CreditApplicationStatus? statusFilter,
        CancellationToken cancellationToken = default)
    {
        await using var context = _contextFactory();

        IQueryable<CreditApplication> query = context.CreditApplications
            .Include(application => application.CreditProduct)
            .Include(application => application.ClientProfile)
                .ThenInclude(profile => profile.User);

        if (statusFilter.HasValue)
        {
            query = query.Where(application => application.Status == statusFilter.Value);
        }

        var applications = await query
            .OrderBy(application => application.CreatedAt)
            .ToListAsync(cancellationToken);

        return applications
            .Select(application => EntityMapper.ToApplicationSummaryDto(application, CalculateMonthlyPayment(application)))
            .ToList();
    }

    /// <inheritdoc />
    public async Task<OperationResult<ApplicationDetailsDto>> GetApplicationDetailsAsync(
        int applicationId,
        CancellationToken cancellationToken = default)
    {
        await using var context = _contextFactory();

        var application = await GetApplicationAsync(context, applicationId, cancellationToken);

        if (application is null)
        {
            return OperationResult<ApplicationDetailsDto>.Failure(ApplicationNotFoundMessage);
        }

        var details = EntityMapper.ToApplicationDetailsDto(
            application,
            CalculateAgeYears(application.ClientProfile),
            CalculateMonthlyPayment(application));

        return OperationResult<ApplicationDetailsDto>.Success(details);
    }

    /// <inheritdoc />
    public async Task<OperationResult<ScoringResultDto>> EvaluateApplicationAsync(
        int applicationId,
        CancellationToken cancellationToken = default)
    {
        await using var context = _contextFactory();

        var application = await GetApplicationAsync(context, applicationId, cancellationToken);

        if (application is null)
        {
            return OperationResult<ScoringResultDto>.Failure(ApplicationNotFoundMessage);
        }

        var scoring = await EvaluateAsync(context, application, cancellationToken);

        return OperationResult<ScoringResultDto>.Success(scoring);
    }

    /// <inheritdoc />
    public async Task<OperationResult<int>> ApproveApplicationAsync(
        int applicationId,
        int specialistUserId,
        string comment,
        CancellationToken cancellationToken = default)
    {
        await using var context = _contextFactory();
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var application = await GetApplicationAsync(context, applicationId, cancellationToken);

        if (application is null)
        {
            return OperationResult<int>.Failure(ApplicationNotFoundMessage);
        }

        if (application.Status != CreditApplicationStatus.New)
        {
            return OperationResult<int>.Failure(ApplicationAlreadyProcessedMessage);
        }

        var scoring = await EvaluateAsync(context, application, cancellationToken);

        if (!scoring.IsApproved)
        {
            return OperationResult<int>.Failure($"Одобрение невозможно. {scoring.Conclusion}");
        }

        var issuedOn = DateOnly.FromDateTime(DateTime.Today);
        var rate = application.CreditProduct.AnnualInterestRate;

        var credit = CreateCredit(application, rate, issuedOn);
        var scheduleItems = CreateScheduleItems(credit, application, rate, issuedOn);

        ApplyApproval(application, specialistUserId, comment, scoring);

        context.Credits.Add(credit);
        context.CreditScheduleItems.AddRange(scheduleItems);
        context.ApplicationStatusHistory.Add(CreateHistoryRecord(
            application,
            CreditApplicationStatus.New,
            CreditApplicationStatus.Approved,
            specialistUserId,
            $"Одобрено. {scoring.Conclusion}"));

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return OperationResult<int>.Success(credit.Id);
    }

    /// <inheritdoc />
    public async Task<OperationResult> RejectApplicationAsync(
        int applicationId,
        int specialistUserId,
        string comment,
        CancellationToken cancellationToken = default)
    {
        await using var context = _contextFactory();

        var application = await GetApplicationAsync(context, applicationId, cancellationToken);

        if (application is null)
        {
            return OperationResult.Failure(ApplicationNotFoundMessage);
        }

        if (application.Status != CreditApplicationStatus.New)
        {
            return OperationResult.Failure(ApplicationAlreadyProcessedMessage);
        }

        if (string.IsNullOrWhiteSpace(comment))
        {
            return OperationResult.Failure(RejectionCommentRequiredMessage);
        }

        var previousStatus = application.Status;

        application.Status = CreditApplicationStatus.Rejected;
        application.ReviewedAt = DateTime.Now;
        application.ReviewedByUserId = specialistUserId;
        application.DecisionComment = comment;

        context.ApplicationStatusHistory.Add(CreateHistoryRecord(
            application,
            previousStatus,
            CreditApplicationStatus.Rejected,
            specialistUserId,
            $"Отклонено. {comment}"));

        await context.SaveChangesAsync(cancellationToken);

        return OperationResult.Success();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CreditSummaryDto>> GetCreditsAsync(
        string searchText,
        CancellationToken cancellationToken = default)
    {
        await using var context = _contextFactory();

        IQueryable<Credit> query = context.Credits
            .Include(credit => credit.ScheduleItems)
            .Include(credit => credit.CreditApplication)
                .ThenInclude(application => application.ClientProfile)
                    .ThenInclude(profile => profile.User)
            .Include(credit => credit.CreditApplication)
                .ThenInclude(application => application.CreditProduct);

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            query = query.Where(credit =>
                credit.CreditApplication.ClientProfile.User.FullName.Contains(searchText) ||
                credit.CreditApplication.CreditProduct.Name.Contains(searchText));
        }

        var credits = await query
            .OrderByDescending(credit => credit.IssuedOn)
            .ToListAsync(cancellationToken);

        return credits
            .Select(credit => EntityMapper.ToCreditSummaryDto(
                credit,
                credit.CreditApplication.ClientProfile.User.FullName,
                credit.CreditApplication.CreditProduct.Name))
            .ToList();
    }

    /// <inheritdoc />
    public async Task<OperationResult<IReadOnlyList<ScheduleRowDto>>> GetScheduleAsync(
        int creditId,
        CancellationToken cancellationToken = default)
    {
        await using var context = _contextFactory();

        var scheduleItems = await context.CreditScheduleItems
            .Where(item => item.CreditId == creditId)
            .OrderBy(item => item.Number)
            .ToListAsync(cancellationToken);

        if (scheduleItems.Count == 0)
        {
            return OperationResult<IReadOnlyList<ScheduleRowDto>>.Failure(CreditNotFoundMessage);
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var rows = scheduleItems.Select(item => EntityMapper.ToScheduleRowDto(item, today)).ToList();

        return OperationResult<IReadOnlyList<ScheduleRowDto>>.Success(rows);
    }

    /// <inheritdoc />
    public async Task<OperationResult> RegisterPaymentAsync(
        PaymentRegistrationDto registration,
        int specialistUserId,
        CancellationToken cancellationToken = default)
    {
        await using var context = _contextFactory();

        var scheduleItem = await context.CreditScheduleItems
            .Include(item => item.Credit)
            .FirstOrDefaultAsync(item => item.Id == registration.CreditScheduleItemId, cancellationToken);

        if (scheduleItem is null || scheduleItem.CreditId != registration.CreditId)
        {
            return OperationResult.Failure("Платёж по указанному кредиту не найден.");
        }

        var validationMessage = ValidatePayment(scheduleItem, registration);

        if (validationMessage is not null)
        {
            return OperationResult.Failure(validationMessage);
        }

        var payment = CreatePayment(scheduleItem, registration, specialistUserId);

        scheduleItem.Status = ScheduleItemStatus.Paid;
        scheduleItem.PaidOn = registration.PaidOn;

        await CloseCreditIfFullyPaidAsync(context, scheduleItem.Credit, scheduleItem.Id, registration.PaidOn, cancellationToken);

        context.Payments.Add(payment);
        await context.SaveChangesAsync(cancellationToken);

        return OperationResult.Success();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PaymentDto>> GetPaymentsAsync(
        int creditId,
        CancellationToken cancellationToken = default)
    {
        await using var context = _contextFactory();

        var payments = await context.Payments
            .Include(payment => payment.RegisteredBy)
            .Where(payment => payment.CreditId == creditId)
            .OrderByDescending(payment => payment.PaidOn)
            .ToListAsync(cancellationToken);

        return payments
            .Select(payment => EntityMapper.ToPaymentDto(payment, payment.RegisteredBy.FullName))
            .ToList();
    }

    /// <summary>
    /// Загружает заявку вместе с анкетой клиента и кредитным продуктом.
    /// </summary>
    /// <param name="context">Контекст базы данных.</param>
    /// <param name="applicationId">Идентификатор заявки.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Заявка либо null, если заявка не найдена.</returns>
    private static Task<CreditApplication?> GetApplicationAsync(
        AppDbContext context,
        int applicationId,
        CancellationToken cancellationToken) =>
        context.CreditApplications
            .Include(application => application.CreditProduct)
            .Include(application => application.ClientProfile)
                // ФИО клиента берётся из связанной учётной записи: без этой загрузки
                // карточка заявки собирается с ошибкой NullReferenceException.
                .ThenInclude(profile => profile.User)
            .FirstOrDefaultAsync(application => application.Id == applicationId, cancellationToken);

    /// <summary>
    /// Выполняет скоринг заёмщика по заявке.
    /// </summary>
    /// <param name="context">Контекст базы данных.</param>
    /// <param name="application">Проверяемая заявка.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Результат оценки заёмщика.</returns>
    private async Task<ScoringResultDto> EvaluateAsync(
        AppDbContext context,
        CreditApplication application,
        CancellationToken cancellationToken)
    {
        var profile = application.ClientProfile;
        var hasOverduePayments = await HasOverduePaymentsAsync(context, profile.Id, cancellationToken);

        var input = new ScoringInput(
            CalculateAgeYears(profile),
            profile.EmploymentMonths,
            profile.MonthlyIncome,
            profile.MonthlyExpenses,
            hasOverduePayments);

        return _scoringService.Evaluate(input, CalculateMonthlyPayment(application));
    }

    /// <summary>
    /// Определяет наличие просроченных платежей по действующим кредитам клиента.
    /// </summary>
    /// <param name="context">Контекст базы данных.</param>
    /// <param name="clientProfileId">Идентификатор анкеты клиента.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Значение, если просроченные платежи есть.</returns>
    private static Task<bool> HasOverduePaymentsAsync(
        AppDbContext context,
        int clientProfileId,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        return context.CreditScheduleItems.AnyAsync(
            item => item.Status == ScheduleItemStatus.Planned
                    && item.DueDate < today
                    && item.Credit.Status == CreditStatus.Active
                    && item.Credit.CreditApplication.ClientProfileId == clientProfileId,
            cancellationToken);
    }

    /// <summary>
    /// Создаёт кредит по одобренной заявке.
    /// </summary>
    /// <param name="application">Одобренная заявка.</param>
    /// <param name="annualInterestRate">Зафиксированная ставка по продукту.</param>
    /// <param name="issuedOn">Дата выдачи кредита.</param>
    /// <returns>Новый кредит.</returns>
    private static Credit CreateCredit(CreditApplication application, decimal annualInterestRate, DateOnly issuedOn) => new()
    {
        CreditApplicationId = application.Id,
        IssuedOn = issuedOn,
        Amount = application.Amount,
        AnnualInterestRate = annualInterestRate,
        TermMonths = application.TermMonths,
        Status = CreditStatus.Active
    };

    /// <summary>
    /// Формирует график платежей по вновь открытому кредиту.
    /// </summary>
    /// <param name="credit">Кредит, для которого строится график.</param>
    /// <param name="application">Заявка, по которой открыт кредит.</param>
    /// <param name="annualInterestRate">Зафиксированная ставка по продукту.</param>
    /// <param name="issuedOn">Дата выдачи кредита.</param>
    /// <returns>Коллекция плановых платежей.</returns>
    private List<CreditScheduleItem> CreateScheduleItems(
        Credit credit,
        CreditApplication application,
        decimal annualInterestRate,
        DateOnly issuedOn)
    {
        var firstPaymentDate = issuedOn.AddMonths(1);

        var drafts = _creditCalculator.BuildSchedule(
            application.Amount,
            annualInterestRate,
            application.TermMonths,
            firstPaymentDate);

        return drafts
            .Select(draft => new CreditScheduleItem
            {
                Credit = credit,
                Number = draft.Number,
                DueDate = draft.DueDate,
                PaymentAmount = draft.PaymentAmount,
                InterestAmount = draft.InterestAmount,
                PrincipalAmount = draft.PrincipalAmount,
                RemainingDebt = draft.RemainingDebt,
                Status = ScheduleItemStatus.Planned
            })
            .ToList();
    }

    /// <summary>
    /// Фиксирует решение специалиста по заявке.
    /// </summary>
    /// <param name="application">Обрабатываемая заявка.</param>
    /// <param name="specialistUserId">Идентификатор специалиста.</param>
    /// <param name="comment">Комментарий по решению.</param>
    /// <param name="scoring">Результат скоринга.</param>
    private static void ApplyApproval(
        CreditApplication application,
        int specialistUserId,
        string comment,
        ScoringResultDto scoring)
    {
        application.Status = CreditApplicationStatus.Approved;
        application.ReviewedAt = DateTime.Now;
        application.ReviewedByUserId = specialistUserId;
        application.ScorePoints = scoring.ScorePoints;
        application.PaymentSharePercent = scoring.PaymentSharePercent;
        application.DecisionComment = string.IsNullOrWhiteSpace(comment) ? scoring.Conclusion : comment;
    }

    /// <summary>
    /// Создаёт запись об изменении статуса заявки.
    /// </summary>
    /// <param name="application">Заявка.</param>
    /// <param name="fromStatus">Статус заявки до решения.</param>
    /// <param name="toStatus">Статус заявки после решения.</param>
    /// <param name="changedByUserId">Идентификатор специалиста.</param>
    /// <param name="comment">Комментарий к решению.</param>
    /// <returns>Новая запись истории.</returns>
    private static ApplicationStatusHistory CreateHistoryRecord(
        CreditApplication application,
        CreditApplicationStatus fromStatus,
        CreditApplicationStatus toStatus,
        int changedByUserId,
        string comment) => new()
        {
            CreditApplicationId = application.Id,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            ChangedAt = DateTime.Now,
            ChangedByUserId = changedByUserId,
            Comment = comment
        };

    /// <summary>
    /// Проверяет корректность регистрируемого платежа.
    /// </summary>
    /// <param name="scheduleItem">Оплачиваемый план графика.</param>
    /// <param name="registration">Данные платежа.</param>
    /// <returns>Текст ошибки или null, если платёж можно зарегистрировать.</returns>
    private static string? ValidatePayment(CreditScheduleItem scheduleItem, PaymentRegistrationDto registration)
    {
        if (registration.Amount <= 0m)
        {
            return "Укажите сумму платежа больше нуля.";
        }

        if (scheduleItem.Status == ScheduleItemStatus.Paid)
        {
            return "Платёж по этому плану графика уже зарегистрирован.";
        }

        if (registration.PaidOn < scheduleItem.Credit.IssuedOn)
        {
            return "Дата платежа раньше даты выдачи кредита.";
        }

        // Платёж не может быть в будущем: иначе просрочка по этому плану
        // исчезла бы из графика раньше времени.
        if (registration.PaidOn > DateOnly.FromDateTime(DateTime.Today))
        {
            return "Нельзя зарегистрировать платёж с датой из будущего.";
        }

        // Учётная модель не умеет хранить частичную оплату: план графика
        // помечается оплаченным целиком. При сумме меньше плановой остаток
        // долга уменьшался бы на всю сумму плана, поэтому расхождение запрещено.
        if (decimal.Abs(registration.Amount - scheduleItem.PaymentAmount) > BankConstants.MoneyTolerance + 0.0001m)
        {
            return $"Сумма платежа должна точно совпадать с плановой: {scheduleItem.PaymentAmount:F2} руб.";
        }

        return null;
    }

    /// <summary>
    /// Создаёт зарегистрированный платёж по графику.
    /// </summary>
    /// <param name="scheduleItem">Оплачиваемый план графика.</param>
    /// <param name="registration">Данные платежа.</param>
    /// <param name="specialistUserId">Идентификатор специалиста.</param>
    /// <returns>Новый платёж.</returns>
    private static Payment CreatePayment(
        CreditScheduleItem scheduleItem,
        PaymentRegistrationDto registration,
        int specialistUserId) => new()
        {
            CreditId = scheduleItem.CreditId,
            CreditScheduleItemId = scheduleItem.Id,
            PaidOn = registration.PaidOn,
            Amount = registration.Amount,
            Status = PaymentStatus.Registered,
            Comment = registration.Comment,
            CreatedAt = DateTime.Now,
            RegisteredByUserId = specialistUserId
        };

    /// <summary>
    /// Закрывает кредит, если оплачены все планы графика.
    /// </summary>
    /// <param name="context">Контекст базы данных.</param>
    /// <param name="credit">Кредит, по которому зарегистрирован платёж.</param>
    /// <param name="paidScheduleItemId">Идентификатор только что оплаченного плана.</param>
    /// <param name="paidOn">Дата платежа.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    private static async Task CloseCreditIfFullyPaidAsync(
        AppDbContext context,
        Credit credit,
        int paidScheduleItemId,
        DateOnly paidOn,
        CancellationToken cancellationToken)
    {
        var unpaidItemsCount = await context.CreditScheduleItems.CountAsync(
            item => item.CreditId == credit.Id
                    && item.Id != paidScheduleItemId
                    && item.Status != ScheduleItemStatus.Paid,
            cancellationToken);

        if (unpaidItemsCount == 0)
        {
            credit.Status = CreditStatus.Closed;
            credit.ClosedOn = paidOn;
        }
    }

    /// <summary>
    /// Рассчитывает ежемесячный платёж по заявке.
    /// </summary>
    /// <param name="application">Заявка с загруженным продуктом.</param>
    /// <returns>Сумма ежемесячного платежа.</returns>
    private decimal CalculateMonthlyPayment(CreditApplication application) =>
        _creditCalculator.CalculateMonthlyPayment(
            application.Amount,
            application.CreditProduct.AnnualInterestRate,
            application.TermMonths);

    /// <summary>
    /// Рассчитывает возраст клиента в полных годах.
    /// </summary>
    /// <param name="profile">Анкета клиента.</param>
    /// <returns>Количество полных лет.</returns>
    private static int CalculateAgeYears(ClientProfile profile)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var age = today.Year - profile.BirthDate.Year;

        if (profile.BirthDate.AddYears(age) > today)
        {
            age--;
        }

        return age;
    }
}
