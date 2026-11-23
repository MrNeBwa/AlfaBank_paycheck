using AlfaBank.Core.Models.Enums;

namespace AlfaBank.Core.Models;

public class Payment
{
    public int Id { get; set; }

    public int CreditId { get; set; }

    public int CreditScheduleItemId { get; set; }

    public DateOnly PaidOn { get; set; }

    public decimal Amount { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Registered;

    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public int RegisteredByUserId { get; set; }

    public Credit Credit { get; set; } = null!;

    public CreditScheduleItem CreditScheduleItem { get; set; } = null!;

    public User RegisteredBy { get; set; } = null!;
}
