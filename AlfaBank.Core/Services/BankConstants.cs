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
    /// Максимально допустимое значение показателя долговой нагрузки (ПДН), %.
    /// </summary>
    /// <remarks>
    /// ПДН считается по единственной формуле, принятой во всём приложении:
    /// ПДН = платёж по кредиту / свободные средства × 100,
    /// где свободные средства = ежемесячный доход − ежемесячные расходы.
    /// </remarks>
    public const decimal PaymentShareMaximumPercent = 50m;

    /// <summary>
    /// Допустимое расхождение при сравнении ПДН с пределом.
    /// Защищает сравнение от ошибок двоичного представления decimal.
    /// </summary>
    public const decimal PaymentShareTolerance = 0.0001m;

    /// <summary>
    /// Значение ПДН, если свободные средства заёмщика равны нулю или отрицательны.
    /// Нагрузка считается предельной, и заявка отклоняется.
    /// </summary>
    public const decimal PaymentSharePercentWithoutFreeFunds = 100m;

    /// <summary>
    /// Значение ПДН, выше которого показатель считается предельным.
    /// Страхует от бессмысленных значений при почти нулевых свободных средствах.
    /// </summary>
    public const decimal PaymentSharePercentLimit = 1000m;

    /// <summary>
    /// Минимальный балл скоринга для одобрения заявки.
    /// </summary>
    public const int ScoreMinimumToApprove = 60;

    /// <summary>
    /// Максимальный балл скоринга: шкала оценки заёмщика от нуля до ста.
    /// </summary>
    public const int ScoreMaximumPoints = 100;

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
    /// Минимальное количество ежемесячных платежей по кредиту, которое должны покрывать
    /// свободные средства заёмщика. Используются те же свободные средства
    /// (доход − расходы), что и база расчёта ПДН.
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
    /// Количество знаков после запятой у процентов в текстах интерфейса.
    /// </summary>
    public const int PercentDigitsInText = 1;

    /// <summary>
    /// Допустимое расхождение денежных сумм при сверке, руб.
    /// </summary>
    public const decimal MoneyTolerance = 0.01m;

    /// <summary>
    /// Порог «жёлтой» зоны показателя долговой нагрузки (ПДН), %.
    /// Значение выше порога, но не выше предела, требует внимания специалиста.
    /// </summary>
    public const decimal PaymentShareWarningPercent = 35m;

    /// <summary>
    /// Максимально допустимая процентная ставка по кредиту, % годовых.
    /// Ограничение защищает расчёт аннуитета: при экстремальной ставке
    /// накопленный множитель переполняется и расчёт падает.
    /// </summary>
    public const decimal InterestRateMaximumPercent = 100m;
}
