using AlfaBank.Core.Services.Contracts;
using AlfaBank.Core.Services.Dtos;

namespace AlfaBank.Core.Services;

public sealed class ScoringService : IScoringService
{
    public ScoringResultDto Evaluate(ScoringInput input, decimal monthlyPayment)
    {
        var reasons = new List<string>();

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

    private static decimal NormalizeExpenses(decimal monthlyExpenses) => Math.Max(0m, monthlyExpenses);

    private static decimal CalculateDisposableIncome(decimal monthlyIncome, decimal monthlyExpenses)
    {
        var disposableIncome = monthlyIncome - monthlyExpenses;

        return disposableIncome < 0m ? 0m : disposableIncome;
    }

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

    private static bool IsAgeAcceptable(int ageYears) =>
        ageYears >= BankConstants.AgeMinimumYears && ageYears <= BankConstants.AgeMaximumYears;

    private static bool IsIncomeAcceptable(decimal disposableIncome, decimal monthlyPayment) =>
        disposableIncome >= monthlyPayment * BankConstants.IncomePaymentsCoverCount;

    private static decimal CalculatePaymentSharePercent(decimal disposableIncome, decimal monthlyPayment)
    {
        if (disposableIncome <= 0m)
        {
            return BankConstants.PaymentSharePercentWithoutFreeFunds;
        }

        var result = monthlyPayment / disposableIncome * BankConstants.PercentFactor;

        return result > BankConstants.PaymentSharePercentLimit
            ? BankConstants.PaymentSharePercentWithoutFreeFunds
            : decimal.Round(result, BankConstants.MoneyScaleDigits, MidpointRounding.AwayFromZero);
    }

    private static bool IsPaymentShareAcceptable(decimal paymentSharePercent) =>
        paymentSharePercent <= BankConstants.PaymentShareMaximumPercent + BankConstants.PaymentShareTolerance;

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

    private static string BuildIncomeCoverReason(decimal disposableIncome, decimal monthlyPayment) =>
        $"Свободные средства после расходов ({BankTextFormatter.Rub(disposableIncome)}) не покрывают требуемое " +
        $"количество ежемесячных платежей по кредиту ({BankConstants.IncomePaymentsCoverCount} × " +
        $"{BankTextFormatter.Rub(monthlyPayment)} = " +
        $"{BankTextFormatter.Rub(monthlyPayment * BankConstants.IncomePaymentsCoverCount)}) — " +
        $"снимается {BankConstants.ScorePointsPerCriterion} баллов.";

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
