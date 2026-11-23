using AlfaBank.Core.Models.Enums;

namespace AlfaBank.Core.Models;

public class CreditScheduleItem
{
    public int Id { get; set; }

    public int CreditId { get; set; }

    public int Number { get; set; }

    public DateOnly DueDate { get; set; }

    public decimal PaymentAmount { get; set; }

    public decimal InterestAmount { get; set; }

    public decimal PrincipalAmount { get; set; }

    public decimal RemainingDebt { get; set; }

    public ScheduleItemStatus Status { get; set; } = ScheduleItemStatus.Planned;

    public DateOnly? PaidOn { get; set; }

    public Credit Credit { get; set; } = null!;

    public Payment? Payment { get; set; }
}
