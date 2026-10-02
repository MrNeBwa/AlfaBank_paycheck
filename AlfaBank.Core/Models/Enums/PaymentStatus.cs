namespace AlfaBank.Core.Models.Enums;

/// <summary>
/// Статус платежа, зарегистрированного кредитным специалистом.
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Платёж зарегистрирован и зачтён в погашение кредита.
    /// </summary>
    Registered = 1,

    /// <summary>
    /// Регистрация платежа отменена специалистом (ошибка ввода).
    /// </summary>
    Cancelled = 2
}
