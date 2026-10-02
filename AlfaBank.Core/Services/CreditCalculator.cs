using AlfaBank.Core.Services.Contracts;
using AlfaBank.Core.Services.Dtos;

namespace AlfaBank.Core.Services;

/// <summary>
/// Кредитный калькулятор аннуитетного типа: равные ежемесячные платежи,
/// в которых постепенно уменьшается доля процентов.
/// </summary>
public sealed class CreditCalculator : ICreditCalculator
{
    /// <summary>
    /// Рассчитывает ежемесячный платёж по кредиту.
    /// </summary>
    /// <param name="amount">Сумма кредита в рублях.</param>
    /// <param name="annualInterestRate">Годовая процентная ставка в процентах.</param>
    /// <param name="termMonths">Срок кредита в месяцах.</param>
    /// <returns>Сумма ежемесячного платежа в рублях.</returns>
    public decimal CalculateMonthlyPayment(decimal amount, decimal annualInterestRate, int termMonths)
    {
        ValidateInput(amount, termMonths);

        var monthlyRate = CalculateMonthlyRate(annualInterestRate);

        if (monthlyRate == 0m)
        {
            return RoundMoney(amount / termMonths);
        }

        var accumulatedFactor = Math.Pow(1d + (double)monthlyRate, termMonths);
        var payment = (double)amount * (double)monthlyRate * accumulatedFactor / (accumulatedFactor - 1d);

        return RoundMoney((decimal)payment);
    }

    /// <summary>
    /// Строит график платежей по кредиту. Последний платёж корректируется
    /// так, чтобы задолженность была погашена полностью.
    /// </summary>
    /// <param name="amount">Сумма кредита в рублях.</param>
    /// <param name="annualInterestRate">Годовая процентная ставка в процентах.</param>
    /// <param name="termMonths">Срок кредита в месяцах.</param>
    /// <param name="firstDueDate">Дата первого планового платежа.</param>
    /// <returns>Коллекция плановых платежей по возрастанию дат.</returns>
    public IReadOnlyList<ScheduleDraft> BuildSchedule(
        decimal amount,
        decimal annualInterestRate,
        int termMonths,
        DateOnly firstDueDate)
    {
        var monthlyRate = CalculateMonthlyRate(annualInterestRate);
        var basePayment = CalculateMonthlyPayment(amount, annualInterestRate, termMonths);

        var schedule = new List<ScheduleDraft>(termMonths);
        var remainingDebt = RoundMoney(amount);

        for (var number = 1; number <= termMonths; number++)
        {
            var payment = CreatePayment(number, termMonths, basePayment, remainingDebt, monthlyRate);
            remainingDebt = RoundMoney(remainingDebt - payment.PrincipalAmount);

            schedule.Add(new ScheduleDraft(
                number,
                firstDueDate.AddMonths(number - 1),
                payment.PaymentAmount,
                payment.InterestAmount,
                payment.PrincipalAmount,
                remainingDebt));
        }

        return schedule;
    }

    /// <summary>
    /// Рассчитывает параметры одного платежа графика.
    /// </summary>
    /// <param name="number">Порядковый номер платежа.</param>
    /// <param name="termMonths">Срок кредита в месяцах.</param>
    /// <param name="basePayment">Базовый ежемесячный платёж.</param>
    /// <param name="remainingDebt">Остаток задолженности на начало периода.</param>
    /// <param name="monthlyRate">Месячная процентная ставка в долях.</param>
    /// <returns>Суммы платежа, процентов и основного долга.</returns>
    private static (decimal PaymentAmount, decimal InterestAmount, decimal PrincipalAmount) CreatePayment(
        int number,
        int termMonths,
        decimal basePayment,
        decimal remainingDebt,
        decimal monthlyRate)
    {
        var interestAmount = RoundMoney(remainingDebt * monthlyRate);
        var isLastPayment = number == termMonths;
        var principalAmount = isLastPayment ? remainingDebt : RoundMoney(basePayment - interestAmount);
        var paymentAmount = isLastPayment ? RoundMoney(principalAmount + interestAmount) : basePayment;

        return (paymentAmount, interestAmount, principalAmount);
    }

    /// <summary>
    /// Переводит годовую процентную ставку в месячную долю.
    /// </summary>
    /// <param name="annualInterestRate">Годовая процентная ставка в процентах.</param>
    /// <returns>Месячная процентная ставка в долях единицы.</returns>
    private static decimal CalculateMonthlyRate(decimal annualInterestRate) =>
        annualInterestRate / BankConstants.PercentFactor / BankConstants.MonthsInYear;

    /// <summary>
    /// Проверяет корректность входных данных расчёта.
    /// </summary>
    /// <param name="amount">Сумма кредита.</param>
    /// <param name="termMonths">Срок кредита.</param>
    private static void ValidateInput(decimal amount, int termMonths)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(amount, 0m);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(termMonths, 0);
    }

    /// <summary>
    /// Округляет денежную сумму до копеек.
    /// </summary>
    /// <param name="value">Исходное значение.</param>
    /// <returns>Значение с двумя знаками после запятой.</returns>
    private static decimal RoundMoney(decimal value) =>
        decimal.Round(value, BankConstants.MoneyScaleDigits, MidpointRounding.AwayFromZero);
}
