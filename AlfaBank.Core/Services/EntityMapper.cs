using AlfaBank.Core.Models;
using AlfaBank.Core.Models.Enums;
using AlfaBank.Core.Services.Dtos;

namespace AlfaBank.Core.Services;

/// <summary>
/// Преобразует сущности доменной модели в объекты представления.
/// Сосредоточивает правила форматирования, чтобы не дублировать их в сервисах.
/// </summary>
public static class EntityMapper
{
    /// <summary>
    /// Преобразует кредитный продукт в объект представления.
    /// </summary>
    /// <param name="product">Кредитный продукт.</param>
    /// <returns>Данные продукта для интерфейса.</returns>
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

    /// <summary>
    /// Преобразует пользователя в объект представления для списка.
    /// </summary>
    /// <param name="user">Пользователь вместе с загруженной анкетой клиента.</param>
    /// <returns>Данные пользователя для интерфейса.</returns>
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

    /// <summary>
    /// Преобразует анкету клиента в данные для формы администратора.
    /// </summary>
    /// <param name="profile">Анкета клиента.</param>
    /// <returns>Данные анкеты.</returns>
    private static ClientProfileInputDto ToClientProfileInput(ClientProfile profile) => new(
        profile.PassportNumber,
        profile.BirthDate,
        profile.RegistrationAddress,
        profile.EmployerName,
        profile.EmploymentMonths,
        profile.MonthlyIncome,
        profile.MonthlyExpenses);

    /// <summary>
    /// Преобразует заявку в краткую карточку списка.
    /// </summary>
    /// <param name="application">Заявка вместе с клиентом и продуктом.</param>
    /// <param name="monthlyPayment">Расчётный ежемесячный платёж.</param>
    /// <returns>Краткая информация о заявке.</returns>
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

    /// <summary>
    /// Преобразует заявку в подробную карточку проверки.
    /// </summary>
    /// <param name="application">Заявка вместе с клиентом и продуктом.</param>
    /// <param name="ageYears">Возраст клиента в полных годах.</param>
    /// <param name="monthlyPayment">Расчётный ежемесячный платёж.</param>
    /// <returns>Подробная информация о заявке.</returns>
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

    /// <summary>
    /// Преобразует платёж графика в строку таблицы графика с учётом текущей даты.
    /// </summary>
    /// <param name="item">Платёж графика.</param>
    /// <param name="today">Текущая дата.</param>
    /// <returns>Строка графика платежей.</returns>
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

    /// <summary>
    /// Определяет состояние платежа графика: просроченным считается платёж,
    /// срок которого истёк, но который не внесён.
    /// </summary>
    /// <param name="item">Платёж графика.</param>
    /// <param name="today">Текущая дата.</param>
    /// <returns>Состояние платежа для интерфейса.</returns>
    private static ScheduleItemStatus ResolveScheduleStatus(CreditScheduleItem item, DateOnly today) =>
        item.Status == ScheduleItemStatus.Planned && item.DueDate < today
            ? ScheduleItemStatus.Overdue
            : item.Status;

    /// <summary>
    /// Преобразует кредит вместе с графиком в карточку портфеля.
    /// </summary>
    /// <param name="credit">Кредит вместе с загруженным графиком.</param>
    /// <param name="clientFullName">ФИО клиента.</param>
    /// <param name="productName">Наименование кредитного продукта.</param>
    /// <returns>Информация о кредите.</returns>
    public static CreditSummaryDto ToCreditSummaryDto(Credit credit, string clientFullName, string productName)
    {
        // Entity Framework не гарантирует порядок загрузки коллекции,
        // поэтому график обязательно упорядочивается по номеру платежа.
        // Иначе «следующий платёж» и остаток долга выбирались бы произвольными.
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

    /// <summary>
    /// Преобразует платёж в строку таблицы платежей.
    /// </summary>
    /// <param name="payment">Зарегистрированный платёж.</param>
    /// <param name="registeredByFullName">ФИО специалиста.</param>
    /// <returns>Информация о платеже.</returns>
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

    /// <summary>
    /// Суммирует значения коллекции денежных сумм.
    /// </summary>
    /// <param name="amounts">Коллекция сумм.</param>
    /// <returns>Итоговая сумма.</returns>
    private static decimal Sum(IEnumerable<decimal> amounts)
    {
        var total = amounts.Sum();

        return decimal.Round(total, BankConstants.MoneyScaleDigits, MidpointRounding.AwayFromZero);
    }
}
