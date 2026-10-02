using AlfaBank.Core.Services.Dtos;

namespace AlfaBank.Core.Services.Contracts;

/// <summary>
/// Рассчитывает параметры кредита по аннуитетной схеме погашения.
/// </summary>
public interface ICreditCalculator
{
    /// <summary>
    /// Рассчитывает ежемесячный платёж по аннуитетной схеме.
    /// </summary>
    /// <param name="amount">Сумма кредита в рублях.</param>
    /// <param name="annualInterestRate">Годовая процентная ставка в процентах.</param>
    /// <param name="termMonths">Срок кредита в месяцах.</param>
    /// <returns>Сумма ежемесячного платежа в рублях.</returns>
    decimal CalculateMonthlyPayment(decimal amount, decimal annualInterestRate, int termMonths);

    /// <summary>
    /// Строит график платежей по кредиту.
    /// </summary>
    /// <param name="amount">Сумма кредита в рублях.</param>
    /// <param name="annualInterestRate">Годовая процентная ставка в процентах.</param>
    /// <param name="termMonths">Срок кредита в месяцах.</param>
    /// <param name="firstDueDate">Дата первого планового платежа.</param>
    /// <returns>Коллекция плановых платежей по возрастанию дат.</returns>
    IReadOnlyList<ScheduleDraft> BuildSchedule(
        decimal amount,
        decimal annualInterestRate,
        int termMonths,
        DateOnly firstDueDate);
}
