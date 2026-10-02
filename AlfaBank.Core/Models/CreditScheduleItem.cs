using AlfaBank.Core.Models.Enums;

namespace AlfaBank.Core.Models;

/// <summary>
/// Плановый платёж в графике погашения кредита.
/// </summary>
public class CreditScheduleItem
{
    /// <summary>
    /// Идентификатор платежа графика.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Идентификатор кредита.
    /// </summary>
    public int CreditId { get; set; }

    /// <summary>
    /// Порядковый номер платежа, начиная с единицы.
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// Дата планового платежа.
    /// </summary>
    public DateOnly DueDate { get; set; }

    /// <summary>
    /// Сумма платежа в рублях (проценты и основной долг).
    /// </summary>
    public decimal PaymentAmount { get; set; }

    /// <summary>
    /// Сумма процентов за период в рублях.
    /// </summary>
    public decimal InterestAmount { get; set; }

    /// <summary>
    /// Сумма погашения основного долга в рублях.
    /// </summary>
    public decimal PrincipalAmount { get; set; }

    /// <summary>
    /// Остаток задолженности после платежа в рублях.
    /// </summary>
    public decimal RemainingDebt { get; set; }

    /// <summary>
    /// Состояние платежа.
    /// </summary>
    public ScheduleItemStatus Status { get; set; } = ScheduleItemStatus.Planned;

    /// <summary>
    /// Дата фактической оплаты. Заполняется после регистрации платежа.
    /// </summary>
    public DateOnly? PaidOn { get; set; }

    /// <summary>
    /// Кредит, частью графика которого является платёж.
    /// </summary>
    public Credit Credit { get; set; } = null!;

    /// <summary>
    /// Платёж, зачтённый в погашение данного плана.
    /// </summary>
    public Payment? Payment { get; set; }
}
