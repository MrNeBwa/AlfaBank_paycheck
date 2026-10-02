using AlfaBank.Core.Services.Contracts;
using AlfaBank.Core.Services.Dtos;

namespace AlfaBank.Core.Services;

/// <summary>
/// Реализация скоринга заёмщика: балльная оценка по четырём критериям
/// и проверка показателя долговой нагрузки (ПДР).
/// </summary>
public sealed class ScoringService : IScoringService
{
    /// <inheritdoc />
    public ScoringResultDto Evaluate(ScoringInput input, decimal monthlyPayment)
    {
        var reasons = new List<string>();
        var scorePoints = CalculateScorePoints(input, monthlyPayment, reasons);
        var paymentSharePercent = CalculatePaymentSharePercent(input.MonthlyIncome, monthlyPayment);
        var isPaymentShareAcceptable = paymentSharePercent <= BankConstants.PaymentShareMaximumPercent;

        if (!isPaymentShareAcceptable)
        {
            reasons.Add(
                $"Платеж по кредиту составляет {paymentSharePercent:0.##}% дохода " +
                $"(допустимо не более {BankConstants.PaymentShareMaximumPercent:0.##}%).");
        }

        var isApproved = isPaymentShareAcceptable && scorePoints >= BankConstants.ScoreMinimumToApprove;

        return new ScoringResultDto(
            scorePoints,
            paymentSharePercent,
            isApproved,
            BuildConclusion(scorePoints, isApproved),
            reasons);
    }

    /// <summary>
    /// Начисляет баллы за каждый выполненный критерий и формирует список замечаний.
    /// </summary>
    /// <param name="input">Данные заёмщика.</param>
    /// <param name="monthlyPayment">Ежемесячный платёж по кредиту.</param>
    /// <param name="reasons">Коллекция, в которую добавляются замечания.</param>
    /// <returns>Итоговый балл скоринга.</returns>
    private static int CalculateScorePoints(ScoringInput input, decimal monthlyPayment, ICollection<string> reasons)
    {
        var scorePoints = 0;

        if (IsAgeAcceptable(input.AgeYears))
        {
            scorePoints += BankConstants.ScorePointsPerCriterion;
        }
        else
        {
            reasons.Add($"Возраст заёмщика {input.AgeYears} лет не соответствует требованию " +
                        $"{BankConstants.AgeMinimumYears}–{BankConstants.AgeMaximumYears} лет.");
        }

        if (input.EmploymentMonths >= BankConstants.EmploymentMinimumMonths)
        {
            scorePoints += BankConstants.ScorePointsPerCriterion;
        }
        else
        {
            reasons.Add($"Стаж работы {input.EmploymentMonths} мес. недостаточен " +
                        $"(требуется не менее {BankConstants.EmploymentMinimumMonths} мес.).");
        }

        if (IsIncomeAcceptable(input.MonthlyIncome, monthlyPayment))
        {
            scorePoints += BankConstants.ScorePointsPerCriterion;
        }
        else
        {
            reasons.Add($"Доход клиента не покрывает {BankConstants.IncomePaymentsCoverCount:0} " +
                        "ежемесячных платежей по кредиту.");
        }

        if (!input.HasOverduePayments)
        {
            scorePoints += BankConstants.ScorePointsPerCriterion;
        }
        else
        {
            reasons.Add("У клиента есть просроченные платежи по действующим кредитам.");
        }

        return scorePoints;
    }

    /// <summary>
    /// Проверяет возраст заёмщика.
    /// </summary>
    /// <param name="ageYears">Возраст заёмщика в полных годах.</param>
    /// <returns>Значение, если возраст соответствует требованиям банка.</returns>
    private static bool IsAgeAcceptable(int ageYears) =>
        ageYears >= BankConstants.AgeMinimumYears && ageYears <= BankConstants.AgeMaximumYears;

    /// <summary>
    /// Проверяет, что доход покрывает требуемое количество платежей по кредиту.
    /// </summary>
    /// <param name="monthlyIncome">Ежемесячный доход заёмщика.</param>
    /// <param name="monthlyPayment">Ежемесячный платёж по кредиту.</param>
    /// <returns>Значение, если доход достаточен.</returns>
    private static bool IsIncomeAcceptable(decimal monthlyIncome, decimal monthlyPayment) =>
        monthlyIncome >= monthlyPayment * BankConstants.IncomePaymentsCoverCount;

    /// <summary>
    /// Рассчитывает показатель долговой нагрузки: долю платежа в ежемесячном доходе.
    /// </summary>
    /// <param name="monthlyIncome">Ежемесячный доход заёмщика.</param>
    /// <param name="monthlyPayment">Ежемесячный платёж по кредиту.</param>
    /// <returns>Значение ПДР в процентах.</returns>
    private static decimal CalculatePaymentSharePercent(decimal monthlyIncome, decimal monthlyPayment)
    {
        if (monthlyIncome <= 0m)
        {
            return 100m;
        }

        return decimal.Round(monthlyPayment / monthlyIncome * BankConstants.PercentFactor, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Формирует итоговый вывод по результатам скоринга.
    /// </summary>
    /// <param name="scorePoints">Итоговый балл.</param>
    /// <param name="isApproved">Признак соответствия правилам банка.</param>
    /// <returns>Текст вывода на русском языке.</returns>
    private static string BuildConclusion(int scorePoints, bool isApproved) =>
        isApproved
            ? $"Заёмщик соответствует требованиям банка, оценка {scorePoints} из 100. Заявку можно одобрить."
            : $"Заёмщик не соответствует требованиям банка, оценка {scorePoints} из 100. Заявку следует отклонить.";
}
