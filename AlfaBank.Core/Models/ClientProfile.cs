namespace AlfaBank.Core.Models;

public class ClientProfile
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string PassportNumber { get; set; } = string.Empty;

    public DateOnly BirthDate { get; set; }

    public string RegistrationAddress { get; set; } = string.Empty;

    public string EmployerName { get; set; } = string.Empty;

    public int EmploymentMonths { get; set; }

    public decimal MonthlyIncome { get; set; }

    public decimal MonthlyExpenses { get; set; }

    public User User { get; set; } = null!;

    public List<CreditApplication> Applications { get; set; } = [];
}
