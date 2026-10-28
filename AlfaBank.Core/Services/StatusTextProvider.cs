using AlfaBank.Core.Models.Enums;

namespace AlfaBank.Core.Services;

public static class StatusTextProvider
{
    public static string GetRoleText(UserRole role) => role switch
    {
        UserRole.Client => "Клиент",
        UserRole.CreditSpecialist => "Кредитный специалист",
        UserRole.Administrator => "Администратор",
        _ => "Не определена"
    };

    public static string GetApplicationStatusText(CreditApplicationStatus status) => status switch
    {
        CreditApplicationStatus.New => "Новая",
        CreditApplicationStatus.Approved => "Одобрена",
        CreditApplicationStatus.Rejected => "Отклонена",
        _ => "Не определён"
    };

    public static string GetCreditStatusText(CreditStatus status) => status switch
    {
        CreditStatus.Active => "Обслуживается",
        CreditStatus.Closed => "Погашен",
        _ => "Не определено"
    };

    public static string GetScheduleItemStatusText(ScheduleItemStatus status) => status switch
    {
        ScheduleItemStatus.Planned => "Ожидается",
        ScheduleItemStatus.Paid => "Оплачен",
        ScheduleItemStatus.Overdue => "Просрочен",
        _ => "Не определено"
    };

    public static string GetPaymentStatusText(PaymentStatus status) => status switch
    {
        PaymentStatus.Registered => "Зарегистрирован",
        PaymentStatus.Cancelled => "Отменён",
        _ => "Не определён"
    };
}
