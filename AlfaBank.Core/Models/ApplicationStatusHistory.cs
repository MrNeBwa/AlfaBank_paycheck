using AlfaBank.Core.Models.Enums;

namespace AlfaBank.Core.Models;

public class ApplicationStatusHistory
{
    public int Id { get; set; }

    public int CreditApplicationId { get; set; }

    public CreditApplicationStatus FromStatus { get; set; }

    public CreditApplicationStatus ToStatus { get; set; }

    public DateTime ChangedAt { get; set; }

    public int ChangedByUserId { get; set; }

    public string Comment { get; set; } = string.Empty;

    public CreditApplication CreditApplication { get; set; } = null!;

    public User ChangedBy { get; set; } = null!;
}
