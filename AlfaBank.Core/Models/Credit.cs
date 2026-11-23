using AlfaBank.Core.Models.Enums;

namespace AlfaBank.Core.Models;

public class Credit
{
    public int Id { get; set; }

    public int CreditApplicationId { get; set; }

    public DateOnly IssuedOn { get; set; }

    public decimal Amount { get; set; }

    public decimal AnnualInterestRate { get; set; }

    public int TermMonths { get; set; }

    public CreditStatus Status { get; set; } = CreditStatus.Active;

    public DateOnly? ClosedOn { get; set; }

    public CreditApplication CreditApplication { get; set; } = null!;

    public List<CreditScheduleItem> ScheduleItems { get; set; } = [];

    public List<Payment> Payments { get; set; } = [];
}
