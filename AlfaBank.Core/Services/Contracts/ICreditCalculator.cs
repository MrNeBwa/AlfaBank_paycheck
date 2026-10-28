using AlfaBank.Core.Services.Dtos;

namespace AlfaBank.Core.Services.Contracts;

public interface ICreditCalculator
{
    decimal CalculateMonthlyPayment(decimal amount, decimal annualInterestRate, int termMonths);

    IReadOnlyList<ScheduleDraft> BuildSchedule(
        decimal amount,
        decimal annualInterestRate,
        int termMonths,
        DateOnly firstDueDate);
}
