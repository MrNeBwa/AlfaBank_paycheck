using AlfaBank.Core.Data;
using AlfaBank.Core.Models;
using AlfaBank.Core.Models.Enums;
using AlfaBank.Core.Results;
using AlfaBank.Core.Services.Contracts;
using AlfaBank.Core.Services.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AlfaBank.Core.Services;

/// <summary>
/// Сервис операций клиента.
/// </summary>
public sealed class ClientService : IClientService
{
    private const string ProfileNotFoundMessage = "Анкета клиента не найдена. Обратитесь к администратору.";
    private const string CreditNotFoundMessage = "Кредит не найден в вашем портфеле.";

    private readonly Func<AppDbContext> _contextFactory;
    private readonly ICreditCalculator _creditCalculator;

    /// <summary>
    /// Создаёт сервис операций клиента.
    /// </summary>
    /// <param name="contextFactory">Фабрика контекстов базы данных.</param>
    /// <param name="creditCalculator">Кредитный калькулятор.</param>
    public ClientService(Func<AppDbContext> contextFactory, ICreditCalculator creditCalculator)
    {
        _contextFactory = contextFactory;
        _creditCalculator = creditCalculator;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductDto>> GetAvailableProductsAsync(CancellationToken cancellationToken = default)
    {
        await using var context = _contextFactory();

        var products = await context.CreditProducts
            .Where(product => product.IsActive)
            .OrderBy(product => product.Name)
            .ToListAsync(cancellationToken);

        return products.Select(EntityMapper.ToProductDto).ToList();
    }

    /// <inheritdoc />
    public async Task<OperationResult> SubmitApplicationAsync(
        int clientProfileId,
        ApplicationSubmissionDto submission,
        CancellationToken cancellationToken = default)
    {
        await using var context = _contextFactory();

        var profileExists = await context.ClientProfiles.AnyAsync(
            profile => profile.Id == clientProfileId,
            cancellationToken);

        if (!profileExists)
        {
            return OperationResult.Failure(ProfileNotFoundMessage);
        }

        var product = await context.CreditProducts.FirstOrDefaultAsync(
            candidate => candidate.Id == submission.CreditProductId,
            cancellationToken);

        if (product is null)
        {
            return OperationResult.Failure("Кредитный продукт не найден.");
        }

        var limitsMessage = ValidateSubmissionLimits(product, submission);

        if (limitsMessage is not null)
        {
            return OperationResult.Failure(limitsMessage);
        }

        var application = new CreditApplication
        {
            ClientProfileId = clientProfileId,
            CreditProductId = product.Id,
            Amount = submission.Amount,
            TermMonths = submission.TermMonths,
            Status = CreditApplicationStatus.New,
            CreatedAt = DateTime.Now,
            DecisionComment = string.Empty
        };

        context.CreditApplications.Add(application);
        await context.SaveChangesAsync(cancellationToken);

        return OperationResult.Success();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ApplicationSummaryDto>> GetMyApplicationsAsync(
        int clientProfileId,
        CancellationToken cancellationToken = default)
    {
        await using var context = _contextFactory();

        var applications = await context.CreditApplications
            .Include(application => application.CreditProduct)
            .Include(application => application.ClientProfile)
                .ThenInclude(profile => profile.User)
            .Where(application => application.ClientProfileId == clientProfileId)
            .OrderByDescending(application => application.CreatedAt)
            .ToListAsync(cancellationToken);

        return applications
            .Select(application => EntityMapper.ToApplicationSummaryDto(
                application,
                _creditCalculator.CalculateMonthlyPayment(
                    application.Amount,
                    application.CreditProduct.AnnualInterestRate,
                    application.TermMonths)))
            .ToList();
    }

    /// <inheritdoc />
    public async Task<OperationResult<ClientOverviewDto>> GetOverviewAsync(
        int clientProfileId,
        CancellationToken cancellationToken = default)
    {
        await using var context = _contextFactory();

        var profileExists = await context.ClientProfiles.AnyAsync(
            profile => profile.Id == clientProfileId,
            cancellationToken);

        if (!profileExists)
        {
            return OperationResult<ClientOverviewDto>.Failure(ProfileNotFoundMessage);
        }

        var applications = await context.CreditApplications
            .Where(application => application.ClientProfileId == clientProfileId)
            .ToListAsync(cancellationToken);

        var credits = await LoadCreditsAsync(context, clientProfileId, cancellationToken);

        var overview = new ClientOverviewDto(
            applications.Count,
            applications.Count(application => application.Status == CreditApplicationStatus.New),
            applications.Count(application => application.Status == CreditApplicationStatus.Approved),
            credits.Count(credit => credit.Status == CreditStatus.Active),
            SumUnpaidPayments(credits),
            credits.Sum(credit => SumPaidPayments(credit)));

        return OperationResult<ClientOverviewDto>.Success(overview);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CreditSummaryDto>> GetMyCreditsAsync(
        int clientProfileId,
        CancellationToken cancellationToken = default)
    {
        await using var context = _contextFactory();

        var credits = await LoadCreditsAsync(context, clientProfileId, cancellationToken);

        return credits
            .Select(credit => EntityMapper.ToCreditSummaryDto(
                credit,
                credit.CreditApplication.ClientProfile.User.FullName,
                credit.CreditApplication.CreditProduct.Name))
            .OrderByDescending(credit => credit.IssuedOn)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<OperationResult<IReadOnlyList<ScheduleRowDto>>> GetMyScheduleAsync(
        int clientProfileId,
        int creditId,
        CancellationToken cancellationToken = default)
    {
        await using var context = _contextFactory();

        var isCreditOwnedByClient = await context.Credits.AnyAsync(
            credit => credit.Id == creditId
                      && credit.CreditApplication.ClientProfileId == clientProfileId,
            cancellationToken);

        if (!isCreditOwnedByClient)
        {
            return OperationResult<IReadOnlyList<ScheduleRowDto>>.Failure(CreditNotFoundMessage);
        }

        var scheduleItems = await context.CreditScheduleItems
            .Where(item => item.CreditId == creditId)
            .OrderBy(item => item.Number)
            .ToListAsync(cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var rows = scheduleItems.Select(item => EntityMapper.ToScheduleRowDto(item, today)).ToList();

        return OperationResult<IReadOnlyList<ScheduleRowDto>>.Success(rows);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PaymentDto>> GetMyPaymentsAsync(
        int clientProfileId,
        CancellationToken cancellationToken = default)
    {
        await using var context = _contextFactory();

        var payments = await context.Payments
            .Include(payment => payment.RegisteredBy)
            .Where(payment => payment.Credit.CreditApplication.ClientProfileId == clientProfileId)
            .OrderByDescending(payment => payment.PaidOn)
            .ToListAsync(cancellationToken);

        return payments
            .Select(payment => EntityMapper.ToPaymentDto(payment, payment.RegisteredBy.FullName))
            .ToList();
    }

    /// <summary>
    /// Загружает кредиты клиента вместе с графиками платежей.
    /// </summary>
    /// <param name="context">Контекст базы данных.</param>
    /// <param name="clientProfileId">Идентификатор анкеты клиента.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Коллекция кредитов клиента.</returns>
    private static Task<List<Credit>> LoadCreditsAsync(
        AppDbContext context,
        int clientProfileId,
        CancellationToken cancellationToken) =>
        context.Credits
            .Include(credit => credit.ScheduleItems)
            .Include(credit => credit.CreditApplication)
                .ThenInclude(application => application.ClientProfile)
                    .ThenInclude(profile => profile.User)
            .Include(credit => credit.CreditApplication)
                .ThenInclude(application => application.CreditProduct)
            .Where(credit => credit.CreditApplication.ClientProfileId == clientProfileId)
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Проверяет соответствие заявки лимитам кредитного продукта.
    /// </summary>
    /// <param name="product">Кредитный продукт.</param>
    /// <param name="submission">Данные заявки.</param>
    /// <returns>Текст ошибки или null, если заявка соответствует лимитам.</returns>
    private static string? ValidateSubmissionLimits(CreditProduct product, ApplicationSubmissionDto submission)
    {
        if (!product.IsActive)
        {
            return "Кредитный продукт временно недоступен.";
        }

        if (submission.Amount < product.MinAmount || submission.Amount > product.MaxAmount)
        {
            return $"Сумма кредита должна быть от {product.MinAmount:0.00} до {product.MaxAmount:0.00} руб.";
        }

        if (submission.TermMonths < product.MinTermMonths || submission.TermMonths > product.MaxTermMonths)
        {
            return $"Срок кредита должен быть от {product.MinTermMonths} до {product.MaxTermMonths} месяцев.";
        }

        return null;
    }

    /// <summary>
    /// Суммирует остаток задолженности по всем кредитам клиента.
    /// </summary>
    /// <param name="credits">Кредиты клиента.</param>
    /// <returns>Остаток задолженности в рублях.</returns>
    private static decimal SumUnpaidPayments(IEnumerable<Credit> credits)
    {
        var total = credits
            .SelectMany(credit => credit.ScheduleItems)
            .Where(item => item.Status != ScheduleItemStatus.Paid)
            .Sum(item => item.PrincipalAmount);

        return decimal.Round(total, BankConstants.MoneyScaleDigits, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Суммирует оплаченные платежи по кредиту.
    /// </summary>
    /// <param name="credit">Кредит с графиком платежей.</param>
    /// <returns>Сумма оплаченных платежей.</returns>
    private static decimal SumPaidPayments(Credit credit)
    {
        var total = credit.ScheduleItems
            .Where(item => item.Status == ScheduleItemStatus.Paid)
            .Sum(item => item.PaymentAmount);

        return decimal.Round(total, BankConstants.MoneyScaleDigits, MidpointRounding.AwayFromZero);
    }
}
