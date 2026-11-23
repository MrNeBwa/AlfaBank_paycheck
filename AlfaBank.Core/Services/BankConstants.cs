namespace AlfaBank.Core.Services;

public static class BankConstants
{
    public const int PasswordMinimumLength = 6;

    public const decimal PaymentShareMaximumPercent = 50m;

    public const decimal PaymentShareTolerance = 0.0001m;

    public const decimal PaymentSharePercentWithoutFreeFunds = 100m;

    public const decimal PaymentSharePercentLimit = 1000m;

    public const int ScoreMinimumToApprove = 60;

    public const int ScoreMaximumPoints = 100;

    public const int ScorePointsPerCriterion = 25;

    public const int AgeMinimumYears = 18;

    public const int AgeMaximumYears = 65;

    public const int EmploymentMinimumMonths = 12;

    public const decimal IncomePaymentsCoverCount = 3m;

    public const int MonthsInYear = 12;

    public const decimal PercentFactor = 100m;

    public const int MoneyScaleDigits = 2;

    public const int PercentDigitsInText = 1;

    public const decimal MoneyTolerance = 0.01m;

    public const decimal PaymentShareWarningPercent = 35m;

    public const decimal InterestRateMaximumPercent = 100m;
}
