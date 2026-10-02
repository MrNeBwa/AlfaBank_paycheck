using AlfaBank.Core.Models.Enums;

namespace AlfaBank.Core.Models;

/// <summary>
/// Платёж, зарегистрированный кредитным специалистом по графику погашения.
/// </summary>
public class Payment
{
    /// <summary>
    /// Идентификатор платежа.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Идентификатор кредита.
    /// </summary>
    public int CreditId { get; set; }

    /// <summary>
    /// Идентификатор платежа графика, в счёт которого зачтён платёж.
    /// </summary>
    public int CreditScheduleItemId { get; set; }

    /// <summary>
    /// Дата фактического внесения денежных средств.
    /// </summary>
    public DateOnly PaidOn { get; set; }

    /// <summary>
    /// Сумма платежа в рублях.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Статус платежа.
    /// </summary>
    public PaymentStatus Status { get; set; } = PaymentStatus.Registered;

    /// <summary>
    /// Комментарий специалиста к платежу.
    /// </summary>
    public string Comment { get; set; } = string.Empty;

    /// <summary>
    /// Дата и время регистрации платежа в системе.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Идентификатор специалиста, зарегистрировавшего платёж.
    /// </summary>
    public int RegisteredByUserId { get; set; }

    /// <summary>
    /// Кредит, в погашение которого зачтён платёж.
    /// </summary>
    public Credit Credit { get; set; } = null!;

    /// <summary>
    /// План графика, погашенный платежом.
    /// </summary>
    public CreditScheduleItem CreditScheduleItem { get; set; } = null!;

    /// <summary>
    /// Специалист, зарегистрировавший платёж.
    /// </summary>
    public User RegisteredBy { get; set; } = null!;
}
