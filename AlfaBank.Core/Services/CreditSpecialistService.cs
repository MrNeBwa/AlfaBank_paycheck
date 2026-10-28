using AlfaBank.Core.Data;
using AlfaBank.Core.Models;
using AlfaBank.Core.Models.Enums;
using AlfaBank.Core.Results;
using AlfaBank.Core.Services.Contracts;
using AlfaBank.Core.Services.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AlfaBank.Core.Services;

public sealed class CreditSpecialistService : ICreditSpecialistService
{
    private const string ApplicationNotFoundMessage = "Заявка не найдена.";
    private const string ApplicationAlreadyProcessedMessage = "Заявка уже обработана другим специалистом.";
    private const string RejectionCommentRequiredMessage = "Укажите причину отказа.";
    private const string CreditNotFoundMessage = "Кредит не найден.";

    private readonly Func<AppDbContext> _contextFactory;
    private readonly ICreditCalculator _creditCalculator;
    private readonly IScoringService _scoringService;

    public CreditSpecialistService(
        Func<AppDbContext> contextFactory,
        ICreditCalculator creditCalculator,
        IScoringService scoringService)
    {
        _contextFactory = contextFactory;
        _creditCalculator = creditCalculator;
        _scoringService = scoringService;
    }

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

    private static Task<CreditApplication?> GetApplicationAsync(
        AppDbContext context,
        int applicationId,
        CancellationToken cancellationToken) =>
        context.CreditApplications
            .Include(application => application.CreditProduct)
            .Include(application => application.ClientProfile)
                // ФИО клиента лежит в связанной учётной записи: без User маппинг заявки падает
                .ThenInclude(profile => profile.User)
            .FirstOrDefaultAsync(application => application.Id == applicationId, cancellationToken);

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

    private static Credit CreateCredit(CreditApplication application, decimal annualInterestRate, DateOnly issuedOn) => new()
    {
        CreditApplicationId = application.Id,
        IssuedOn = issuedOn,
        Amount = application.Amount,
        AnnualInterestRate = annualInterestRate,
        TermMonths = application.TermMonths,
        Status = CreditStatus.Active
    };

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

        if (registration.PaidOn > DateOnly.FromDateTime(DateTime.Today))
        {
            return "Нельзя зарегистрировать платёж с датой из будущего.";
        }

        if (decimal.Abs(registration.Amount - scheduleItem.PaymentAmount) > BankConstants.MoneyTolerance + 0.0001m)
        {
            return $"Сумма платежа должна точно совпадать с плановой: {BankTextFormatter.Rub(scheduleItem.PaymentAmount)}.";
        }

        return null;
    }

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

    private decimal CalculateMonthlyPayment(CreditApplication application) =>
        _creditCalculator.CalculateMonthlyPayment(
            application.Amount,
            application.CreditProduct.AnnualInterestRate,
            application.TermMonths);

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
