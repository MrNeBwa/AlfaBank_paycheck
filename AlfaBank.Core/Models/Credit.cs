using AlfaBank.Core.Models.Enums;

namespace AlfaBank.Core.Models;

/// <summary>
/// Кредит, открытый по одобренной заявке клиента.
/// </summary>
public class Credit
{
    /// <summary>
    /// Идентификатор кредита.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Идентификатор заявки, по которой открыт кредит.
    /// </summary>
    public int CreditApplicationId { get; set; }

    /// <summary>
    /// Дата выдачи кредита.
    /// </summary>
    public DateOnly IssuedOn { get; set; }

    /// <summary>
    /// Сумма выданного кредита в рублях.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Зафиксированная годовая процентная ставка в процентах.
    /// </summary>
    public decimal AnnualInterestRate { get; set; }

    /// <summary>
    /// Срок кредита в месяцах.
    /// </summary>
    public int TermMonths { get; set; }

    /// <summary>
    /// Текущее состояние кредита.
    /// </summary>
    public CreditStatus Status { get; set; } = CreditStatus.Active;

    /// <summary>
    /// Дата полного погашения кредита. Заполняется при закрытии кредита.
    /// </summary>
    public DateOnly? ClosedOn { get; set; }

    /// <summary>
    /// Заявка, по которой открыт кредит.
    /// </summary>
    public CreditApplication CreditApplication { get; set; } = null!;

    /// <summary>
    /// График платежей по кредиту.
    /// </summary>
    public List<CreditScheduleItem> ScheduleItems { get; set; } = [];

    /// <summary>
    /// Платёжи, зарегистрированные по кредиту.
    /// </summary>
    public List<Payment> Payments { get; set; } = [];
}
