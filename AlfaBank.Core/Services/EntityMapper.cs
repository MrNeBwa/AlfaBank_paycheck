using AlfaBank.Core.Models;
using AlfaBank.Core.Models.Enums;
using AlfaBank.Core.Services.Dtos;

namespace AlfaBank.Core.Services;

public static class EntityMapper
{
    public static ProductDto ToProductDto(CreditProduct product) => new(
        product.Id,
        product.Name,
        product.AnnualInterestRate,
        product.MinAmount,
        product.MaxAmount,
        product.MinTermMonths,
        product.MaxTermMonths,
        product.Description,
        product.IsActive);

    public static UserDto ToUserDto(User user) => new(
        user.Id,
        user.Login,
        user.FullName,
        user.Role,
        StatusTextProvider.GetRoleText(user.Role),
        user.IsActive,
        user.ClientProfile is not null,
        user.CreatedAt,
        user.ClientProfile is null ? null : ToClientProfileInput(user.ClientProfile));

    private static ClientProfileInputDto ToClientProfileInput(ClientProfile profile) => new(
        profile.PassportNumber,
        profile.BirthDate,
        profile.RegistrationAddress,
        profile.EmployerName,
        profile.EmploymentMonths,
        profile.MonthlyIncome,
        profile.MonthlyExpenses);

    public static ApplicationSummaryDto ToApplicationSummaryDto(CreditApplication application, decimal monthlyPayment) => new(
        application.Id,
        application.ClientProfile.User.FullName,
        application.CreditProduct.Name,
        application.Amount,
        application.TermMonths,
        application.Status,
        StatusTextProvider.GetApplicationStatusText(application.Status),
        monthlyPayment,
        application.DecisionComment,
        application.CreatedAt);

    public static ApplicationDetailsDto ToApplicationDetailsDto(
        CreditApplication application,
        int ageYears,
        decimal monthlyPayment) => new(
        application.Id,
        application.ClientProfile.User.FullName,
        application.ClientProfile.PassportNumber,
        application.ClientProfile.BirthDate,
        ageYears,
        application.ClientProfile.RegistrationAddress,
        application.ClientProfile.EmployerName,
        application.ClientProfile.EmploymentMonths,
        application.ClientProfile.MonthlyIncome,
        application.ClientProfile.MonthlyExpenses,
        application.CreditProduct.Name,
        application.CreditProduct.AnnualInterestRate,
        application.Amount,
        application.TermMonths,
        monthlyPayment,
        application.Status,
        StatusTextProvider.GetApplicationStatusText(application.Status),
        application.DecisionComment,
        application.ScorePoints,
        application.PaymentSharePercent,
        application.CreatedAt,
        application.ReviewedAt);

    public static ScheduleRowDto ToScheduleRowDto(CreditScheduleItem item, DateOnly today) => new(
        item.Id,
        item.Number,
        item.DueDate,
        item.PaymentAmount,
        item.InterestAmount,
        item.PrincipalAmount,
        item.RemainingDebt,
        ResolveScheduleStatus(item, today),
        StatusTextProvider.GetScheduleItemStatusText(ResolveScheduleStatus(item, today)),
        item.PaidOn);

    private static ScheduleItemStatus ResolveScheduleStatus(CreditScheduleItem item, DateOnly today) =>
        item.Status == ScheduleItemStatus.Planned && item.DueDate < today
            ? ScheduleItemStatus.Overdue
            : item.Status;

    public static CreditSummaryDto ToCreditSummaryDto(Credit credit, string clientFullName, string productName)
    {
        var schedule = credit.ScheduleItems
            .OrderBy(item => item.Number)
            .ToList();

        var paidItems = schedule.Where(item => item.Status == ScheduleItemStatus.Paid).ToList();
        var nextItem = schedule.FirstOrDefault(item => item.Status != ScheduleItemStatus.Paid);

        return new CreditSummaryDto(
            credit.Id,
            credit.CreditApplicationId,
            clientFullName,
            productName,
            credit.IssuedOn,
            credit.Amount,
            credit.AnnualInterestRate,
            credit.TermMonths,
            credit.Status,
            StatusTextProvider.GetCreditStatusText(credit.Status),
            Sum(paidItems.Select(item => item.PaymentAmount)),
            nextItem?.RemainingDebt ?? schedule.LastOrDefault()?.RemainingDebt ?? 0m,
            paidItems.Count,
            schedule.Count,
            nextItem?.DueDate,
            nextItem?.PaymentAmount ?? 0m);
    }

    public static PaymentDto ToPaymentDto(Payment payment, string registeredByFullName) => new(
        payment.Id,
        payment.CreditId,
        payment.CreditScheduleItemId,
        payment.PaidOn,
        payment.Amount,
        payment.Status,
        StatusTextProvider.GetPaymentStatusText(payment.Status),
        payment.Comment,
        registeredByFullName);

    private static decimal Sum(IEnumerable<decimal> amounts)
    {
        var total = amounts.Sum();

        return decimal.Round(total, BankConstants.MoneyScaleDigits, MidpointRounding.AwayFromZero);
    }
}
