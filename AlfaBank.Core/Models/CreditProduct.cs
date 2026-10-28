namespace AlfaBank.Core.Models;

public class CreditProduct
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal AnnualInterestRate { get; set; }

    public decimal MinAmount { get; set; }

    public decimal MaxAmount { get; set; }

    public int MinTermMonths { get; set; }

    public int MaxTermMonths { get; set; }

    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public List<CreditApplication> Applications { get; set; } = [];
}
