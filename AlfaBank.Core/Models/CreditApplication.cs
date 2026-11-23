using AlfaBank.Core.Models.Enums;

namespace AlfaBank.Core.Models;

public class CreditApplication
{
    public int Id { get; set; }

    public int ClientProfileId { get; set; }

    public int CreditProductId { get; set; }

    public decimal Amount { get; set; }

    public int TermMonths { get; set; }

    public CreditApplicationStatus Status { get; set; } = CreditApplicationStatus.New;

    public DateTime CreatedAt { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public int? ReviewedByUserId { get; set; }

    public string DecisionComment { get; set; } = string.Empty;

    public int? ScorePoints { get; set; }

    public decimal? PaymentSharePercent { get; set; }

    public ClientProfile ClientProfile { get; set; } = null!;

    public CreditProduct CreditProduct { get; set; } = null!;

    public User? ReviewedBy { get; set; }

    public Credit? Credit { get; set; }

    public List<ApplicationStatusHistory> StatusHistory { get; set; } = [];
}
