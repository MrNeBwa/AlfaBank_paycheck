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

        // Долговая нагрузка оценивается от свободных средств, а не от всего дохода:
        // текущие расходы заёмщика уменьшаются наравне с платежом по кредиту.
        var disposableIncome = CalculateDisposableIncome(input);

        var scorePoints = CalculateScorePoints(input, disposableIncome, monthlyPayment, reasons);
        var paymentSharePercent = CalculatePaymentSharePercent(disposableIncome, monthlyPayment);
        var isPaymentShareAcceptable = paymentSharePercent <= BankConstants.PaymentShareMaximumPercent + 0.0001m;

        if (!isPaymentShareAcceptable)
        {
            reasons.Add(
                $"Платёж по кредиту составляет {paymentSharePercent:F1}% свободных средств " +
                $"(доход {input.MonthlyIncome:F2} руб. − расходы {input.MonthlyExpenses:F2} руб. = " +
                $"{disposableIncome:F2} руб., допустимо не более {BankConstants.PaymentShareMaximumPercent:F0}%).");
        }

        var isApproved = isPaymentShareAcceptable && scorePoints >= BankConstants.ScoreMinimumToApprove;

        return new ScoringResultDto(
            scorePoints,
            paymentSharePercent,
            disposableIncome,
            monthlyPayment,
            isApproved,
            BuildConclusion(scorePoints, isApproved),
            reasons);
    }

    /// <summary>
    /// Рассчитывает свободные средства заёмщика: ежемесячный доход за вычетом расходов.
    /// </summary>
    /// <param name="input">Данные заёмщика.</param>
    /// <returns>Свободные средства в месяц.</returns>
    private static decimal CalculateDisposableIncome(ScoringInput input)
    {
        var disposableIncome = input.MonthlyIncome - Math.Max(0m, input.MonthlyExpenses);

        return disposableIncome < 0m ? 0m : disposableIncome;
    }

    /// <summary>
    /// Начисляет баллы за каждый выполненный критерий и формирует список замечаний.
    /// </summary>
    /// <param name="input">Данные заёмщика.</param>
    /// <param name="disposableIncome">Свободные средства заёмщика после текущих расходов.</param>
    /// <param name="monthlyPayment">Ежемесячный платёж по кредиту.</param>
    /// <param name="reasons">Коллекция, в которую добавляются замечания.</param>
    /// <returns>Итоговый балл скоринга.</returns>
    private static int CalculateScorePoints(
        ScoringInput input,
        decimal disposableIncome,
        decimal monthlyPayment,
        ICollection<string> reasons)
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

        if (IsIncomeAcceptable(disposableIncome, monthlyPayment))
        {
            scorePoints += BankConstants.ScorePointsPerCriterion;
        }
        else
        {
            reasons.Add($"Свободные средства после расходов ({disposableIncome:0.##} руб.) не покрывают " +
                        $"{BankConstants.IncomePaymentsCoverCount:0} ежемесячных платежей по кредиту.");
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
    /// Проверяет, что свободные средства покрывают требуемое количество платежей по кредиту.
    /// </summary>
    /// <param name="disposableIncome">Свободные средства заёмщика после текущих расходов.</param>
    /// <param name="monthlyPayment">Ежемесячный платёж по кредиту.</param>
    /// <returns>Значение, если свободных средств достаточно.</returns>
    private static bool IsIncomeAcceptable(decimal disposableIncome, decimal monthlyPayment) =>
        disposableIncome >= monthlyPayment * BankConstants.IncomePaymentsCoverCount;

    /// <summary>
    /// Рассчитывает показатель долговой нагрузки: долю платежа в свободных средствах.
    /// </summary>
    /// <param name="disposableIncome">Свободные средства заёмщика после текущих расходов.</param>
    /// <param name="monthlyPayment">Ежемесячный платёж по кредиту.</param>
    /// <returns>Значение ПДР в процентах.</returns>
    private static decimal CalculatePaymentSharePercent(decimal disposableIncome, decimal monthlyPayment)
    {
        if (disposableIncome <= 0m)
        {
            // Свободных средств нет: нагрузка предельная, заявка отклоняется.
            return BankConstants.PercentFactor;
        }

        var result = monthlyPayment / disposableIncome * BankConstants.PercentFactor;
        return result > BankConstants.PercentFactor * 1000m ? BankConstants.PercentFactor : decimal.Round(result, 2, MidpointRounding.AwayFromZero);
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
