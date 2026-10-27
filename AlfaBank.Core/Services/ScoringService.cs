using AlfaBank.Core.Services.Contracts;
using AlfaBank.Core.Services.Dtos;

namespace AlfaBank.Core.Services;

/// <summary>
/// Реализация скоринга заёмщика: балльная оценка по четырём критериям
/// и проверка показателя долговой нагрузки (ПДН).
/// </summary>
/// <remarks>
/// Формула ПДН в приложении единственная:
/// ПДН = платёж по кредиту / свободные средства × 100,
/// свободные средства = ежемесячный доход − ежемесячные расходы.
/// Эта же база используется в критерии «свободные средства покрывают
/// требуемое количество платежей», поэтому показатель и замечания
/// не могут опираться на разные определения.
/// </remarks>
public sealed class ScoringService : IScoringService
{
    /// <inheritdoc />
    public ScoringResultDto Evaluate(ScoringInput input, decimal monthlyPayment)
    {
        var reasons = new List<string>();

        // Расходы отрицательными быть не могут: иначе свободные средства
        // завышались бы, а ПДН занижался.
        var monthlyExpenses = NormalizeExpenses(input.MonthlyExpenses);
        var disposableIncome = CalculateDisposableIncome(input.MonthlyIncome, monthlyExpenses);

        var scorePoints = CalculateScorePoints(input, disposableIncome, monthlyPayment, reasons);
        var paymentSharePercent = CalculatePaymentSharePercent(disposableIncome, monthlyPayment);
        var isPaymentShareAcceptable = IsPaymentShareAcceptable(paymentSharePercent);

        if (!isPaymentShareAcceptable)
        {
            reasons.Add(BuildPaymentShareReason(input, monthlyExpenses, disposableIncome, monthlyPayment, paymentSharePercent));
        }

        var isScoreSufficient = scorePoints >= BankConstants.ScoreMinimumToApprove;
        var isApproved = isPaymentShareAcceptable && isScoreSufficient;

        return new ScoringResultDto(
            scorePoints,
            paymentSharePercent,
            disposableIncome,
            monthlyPayment,
            isPaymentShareAcceptable,
            isApproved,
            BuildConclusion(scorePoints, paymentSharePercent, isPaymentShareAcceptable, isScoreSufficient),
            reasons);
    }

    /// <summary>
    /// Приводит ежемесячные расходы к неотрицательному значению.
    /// </summary>
    /// <param name="monthlyExpenses">Расходы из анкеты заёмщика.</param>
    /// <returns>Расходы, не меньшие нуля.</returns>
    private static decimal NormalizeExpenses(decimal monthlyExpenses) => Math.Max(0m, monthlyExpenses);

    /// <summary>
    /// Рассчитывает свободные средства заёмщика: ежемесячный доход за вычетом расходов.
    /// </summary>
    /// <param name="monthlyIncome">Ежемесячный доход заёмщика.</param>
    /// <param name="monthlyExpenses">Ежемесячные расходы заёмщика.</param>
    /// <returns>Свободные средства в месяц.</returns>
    private static decimal CalculateDisposableIncome(decimal monthlyIncome, decimal monthlyExpenses)
    {
        var disposableIncome = monthlyIncome - monthlyExpenses;

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
            reasons.Add(BuildIncomeCoverReason(disposableIncome, monthlyPayment));
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
    /// База расчёта совпадает с базой расчёта ПДН.
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
    /// <returns>Значение ПДН в процентах.</returns>
    private static decimal CalculatePaymentSharePercent(decimal disposableIncome, decimal monthlyPayment)
    {
        if (disposableIncome <= 0m)
        {
            // Свободных средств нет: нагрузка предельная, заявка отклоняется.
            return BankConstants.PaymentSharePercentWithoutFreeFunds;
        }

        var result = monthlyPayment / disposableIncome * BankConstants.PercentFactor;

        return result > BankConstants.PaymentSharePercentLimit
            ? BankConstants.PaymentSharePercentWithoutFreeFunds
            : decimal.Round(result, BankConstants.MoneyScaleDigits, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Определяет, укладывается ли долговая нагрузка в предел банка.
    /// </summary>
    /// <param name="paymentSharePercent">Значение ПДН в процентах.</param>
    /// <returns>Значение, если ПДН не превышает предел.</returns>
    private static bool IsPaymentShareAcceptable(decimal paymentSharePercent) =>
        paymentSharePercent <= BankConstants.PaymentShareMaximumPercent + BankConstants.PaymentShareTolerance;

    /// <summary>
    /// Формирует замечание о превышении предела долговой нагрузки.
    /// </summary>
    /// <param name="input">Данные заёмщика.</param>
    /// <param name="monthlyExpenses">Ежемесячные расходы заёмщика.</param>
    /// <param name="disposableIncome">Свободные средства заёмщика после текущих расходов.</param>
    /// <param name="monthlyPayment">Ежемесячный платёж по кредиту.</param>
    /// <param name="paymentSharePercent">Значение ПДН в процентах.</param>
    /// <returns>Текст замечания на русском языке.</returns>
    private static string BuildPaymentShareReason(
        ScoringInput input,
        decimal monthlyExpenses,
        decimal disposableIncome,
        decimal monthlyPayment,
        decimal paymentSharePercent) =>
        $"Показатель долговой нагрузки (ПДН) {BankTextFormatter.Percent(paymentSharePercent)} превышает предел " +
        $"{BankTextFormatter.Percent(BankConstants.PaymentShareMaximumPercent, digits: 0)}: платёж " +
        $"{BankTextFormatter.Rub(monthlyPayment)} при свободных средствах " +
        $"{BankTextFormatter.Rub(disposableIncome)} в месяц (доход " +
        $"{BankTextFormatter.Rub(input.MonthlyIncome)} − расходы " +
        $"{BankTextFormatter.Rub(monthlyExpenses)}).";

    /// <summary>
    /// Формирует замечание о непокрытии платежей свободными средствами.
    /// Невыполнение этого критерия снижает балл, но само по себе не является
    /// причиной отказа, поэтому это указано прямо: иначе замечание
    /// противоречило бы выводу «заявку можно одобрить».
    /// </summary>
    /// <param name="disposableIncome">Свободные средства заёмщика после текущих расходов.</param>
    /// <param name="monthlyPayment">Ежемесячный платёж по кредиту.</param>
    /// <returns>Текст замечания на русском языке.</returns>
    private static string BuildIncomeCoverReason(decimal disposableIncome, decimal monthlyPayment) =>
        $"Свободные средства после расходов ({BankTextFormatter.Rub(disposableIncome)}) не покрывают требуемое " +
        $"количество ежемесячных платежей по кредиту ({BankConstants.IncomePaymentsCoverCount} × " +
        $"{BankTextFormatter.Rub(monthlyPayment)} = " +
        $"{BankTextFormatter.Rub(monthlyPayment * BankConstants.IncomePaymentsCoverCount)}) — " +
        $"снимается {BankConstants.ScorePointsPerCriterion} баллов.";

    /// <summary>
    /// Формирует итоговый вывод по результатам скоринга с указанием причины отказа.
    /// </summary>
    /// <param name="scorePoints">Итоговый балл.</param>
    /// <param name="paymentSharePercent">Значение ПДН в процентах.</param>
    /// <param name="isPaymentShareAcceptable">Признак того, что ПДН укладывается в предел.</param>
    /// <param name="isScoreSufficient">Признак того, что баллов достаточно для одобрения.</param>
    /// <returns>Текст вывода на русском языке.</returns>
    private static string BuildConclusion(
        int scorePoints,
        decimal paymentSharePercent,
        bool isPaymentShareAcceptable,
        bool isScoreSufficient)
    {
        var scoreText = $"{scorePoints} из {BankConstants.ScoreMaximumPoints}";

        if (isScoreSufficient && isPaymentShareAcceptable)
        {
            return $"Заёмщик соответствует требованиям банка, оценка {scoreText}. Заявку можно одобрить.";
        }

        // Причина отказа называется прямо: без неё вывод «не соответствует требованиям банка»
        // противоречил бы блоку с замечаниями и выглядел как ошибка расчёта.
        var paymentShareText =
            $"платёж превышает допустимую долю свободных средств " +
            $"({BankTextFormatter.Percent(paymentSharePercent)} при пределе " +
            $"{BankTextFormatter.Percent(BankConstants.PaymentShareMaximumPercent, digits: 0)})";

        if (isScoreSufficient)
        {
            return $"Оценка {scoreText} достаточна, но {paymentShareText}. Заявку следует отклонить.";
        }

        var scoreTextWithThreshold =
            $"Оценка {scoreText} ниже минимально допустимой ({BankConstants.ScoreMinimumToApprove})";

        return isPaymentShareAcceptable
            ? $"{scoreTextWithThreshold}. Заявку следует отклонить."
            : $"{scoreTextWithThreshold}, и {paymentShareText}. Заявку следует отклонить.";
    }
}
