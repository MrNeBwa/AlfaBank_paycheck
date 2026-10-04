namespace AlfaBank.Core.Services;

/// <summary>
/// Правила и числовые ограничения банковской логики.
/// Значения вынесены в отдельный класс, чтобы исключить «магические числа» в коде.
/// </summary>
public static class BankConstants
{
    /// <summary>
    /// Минимальная длина пароля пользователя.
    /// </summary>
    public const int PasswordMinimumLength = 6;

    /// <summary>
    /// Максимально допустимая доля платежа по кредиту в свободных средствах (ПДР), %.
    /// </summary>
    public const decimal PaymentShareMaximumPercent = 50m;

    /// <summary>
    /// Минимальный балл скоринга для одобрения заявки.
    /// </summary>
    public const int ScoreMinimumToApprove = 60;

    /// <summary>
    /// Количество баллов за один выполненный критерий скоринга.
    /// </summary>
    public const int ScorePointsPerCriterion = 25;

    /// <summary>
    /// Минимальный возраст заёмщика, лет.
    /// </summary>
    public const int AgeMinimumYears = 18;

    /// <summary>
    /// Максимальный возраст заёмщика, лет.
    /// </summary>
    public const int AgeMaximumYears = 65;

    /// <summary>
    /// Минимальный стаж работы на текущем месте, месяцев.
    /// </summary>
    public const int EmploymentMinimumMonths = 12;

    /// <summary>
    /// Минимальное количество ежемесячных платежей, которое должен покрывать доход заёмщика.
    /// </summary>
    public const decimal IncomePaymentsCoverCount = 3m;

    /// <summary>
    /// Количество месяцев в году.
    /// </summary>
    public const int MonthsInYear = 12;

    /// <summary>
    /// Делитель для перевода процентов в десятичную дробь.
    /// </summary>
    public const decimal PercentFactor = 100m;

    /// <summary>
    /// Количество знаков после запятой у денежных сумм.
    /// </summary>
    public const int MoneyScaleDigits = 2;

    /// <summary>
    /// Допустимое расхождение денежных сумм при сверке, руб.
    /// </summary>
    public const decimal MoneyTolerance = 0.01m;

    /// <summary>
    /// Порог «жёлтой» зоны показателя долговой нагрузки (ПДР), %.
    /// </summary>
    public const decimal PaymentShareWarningPercent = 35m;

    /// <summary>
    /// Максимально допустимая процентная ставка по кредиту, % годовых.
    /// Ограничение защищает расчёт аннуитета: при экстремальной ставке
    /// накопленный множитель переполняется и расчёт падает.
    /// </summary>
    public const decimal InterestRateMaximumPercent = 100m;
}
