using AlfaBank.Core.Models.Enums;

namespace AlfaBank.Core.Services;

/// <summary>
/// Формирует русскоязычные названия значений перечислений доменной модели.
/// </summary>
public static class StatusTextProvider
{
    /// <summary>
    /// Возвращает название роли пользователя.
    /// </summary>
    /// <param name="role">Роль пользователя.</param>
    /// <returns>Название роли на русском языке.</returns>
    public static string GetRoleText(UserRole role) => role switch
    {
        UserRole.Client => "Клиент",
        UserRole.CreditSpecialist => "Кредитный специалист",
        UserRole.Administrator => "Администратор",
        _ => "Не определена"
    };

    /// <summary>
    /// Возвращает название статуса заявки.
    /// </summary>
    /// <param name="status">Статус заявки.</param>
    /// <returns>Название статуса на русском языке.</returns>
    public static string GetApplicationStatusText(CreditApplicationStatus status) => status switch
    {
        CreditApplicationStatus.New => "Новая",
        CreditApplicationStatus.Approved => "Одобрена",
        CreditApplicationStatus.Rejected => "Отклонена",
        _ => "Не определён"
    };

    /// <summary>
    /// Возвращает название состояния кредита.
    /// </summary>
    /// <param name="status">Состояние кредита.</param>
    /// <returns>Название состояния на русском языке.</returns>
    public static string GetCreditStatusText(CreditStatus status) => status switch
    {
        CreditStatus.Active => "Обслуживается",
        CreditStatus.Closed => "Погашен",
        _ => "Не определено"
    };

    /// <summary>
    /// Возвращает название состояния платежа графика.
    /// </summary>
    /// <param name="status">Состояние платежа.</param>
    /// <returns>Название состояния на русском языке.</returns>
    public static string GetScheduleItemStatusText(ScheduleItemStatus status) => status switch
    {
        ScheduleItemStatus.Planned => "Ожидается",
        ScheduleItemStatus.Paid => "Оплачен",
        ScheduleItemStatus.Overdue => "Просрочен",
        _ => "Не определено"
    };

    /// <summary>
    /// Возвращает название статуса зарегистрированного платежа.
    /// </summary>
    /// <param name="status">Статус платежа.</param>
    /// <returns>Название статуса на русском языке.</returns>
    public static string GetPaymentStatusText(PaymentStatus status) => status switch
    {
        PaymentStatus.Registered => "Зарегистрирован",
        PaymentStatus.Cancelled => "Отменён",
        _ => "Не определён"
    };
}
