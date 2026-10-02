namespace AlfaBank.Core.Data;

/// <summary>
/// Типовые ограничения схемы базы данных: длины строк и точность денежных значений.
/// </summary>
public static class ColumnLimits
{
    /// <summary>
    /// Максимальная длина логина.
    /// </summary>
    public const int Login = 100;

    /// <summary>
    /// Максимальная длина полного имени.
    /// </summary>
    public const int FullName = 200;

    /// <summary>
    /// Максимальная длина хэша пароля вместе с параметрами алгоритма.
    /// </summary>
    public const int PasswordHash = 512;

    /// <summary>
    /// Максимальная длина наименования кредитного продукта.
    /// </summary>
    public const int ProductName = 150;

    /// <summary>
    /// Максимальная длина краткого описания или комментария.
    /// </summary>
    public const int ShortDescription = 500;

    /// <summary>
    /// Максимальная длина комментария к решению или платежу.
    /// </summary>
    public const int Comment = 1000;

    /// <summary>
    /// Максимальная длина номера паспорта.
    /// </summary>
    public const int PassportNumber = 30;

    /// <summary>
    /// Максимальная длина адреса и наименования организации.
    /// </summary>
    public const int Address = 300;

    /// <summary>
    /// Максимальное число разрядов денежной суммы.
    /// </summary>
    public const int MoneyPrecision = 18;

    /// <summary>
    /// Количество знаков после запятой у денежных сумм.
    /// </summary>
    public const int MoneyScale = 2;

    /// <summary>
    /// Максимальное число разрядов процентной ставки.
    /// </summary>
    public const int RatePrecision = 5;
}
