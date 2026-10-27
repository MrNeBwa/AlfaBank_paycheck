using System.Globalization;

namespace AlfaBank.Core.Services;

/// <summary>
/// Единое текстовое форматирование чисел для замечаний, выводов и подписей.
/// </summary>
/// <remarks>
/// Тексты скоринга и модели представления должны показывать суммы и проценты
/// одинаково: «38 190,45 руб.» и «50,9 %». Собственные форматы вида {0:F2}
/// дают числа без разделителя тысяч и зависят от текущей культуры процесса,
/// поэтому все значения проходят через этот класс.
/// </remarks>
public static class BankTextFormatter
{
    /// <summary>
    /// Культура оформления текстов: русская, с запятой в качестве десятичного разделителя.
    /// </summary>
    public static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("ru-RU");

    /// <summary>
    /// Форматирует денежную сумму с разделителем тысяч и двумя знаками после запятой.
    /// </summary>
    /// <param name="value">Денежная сумма.</param>
    /// <returns>Сумма в виде «38 190,45».</returns>
    public static string Money(decimal value) =>
        value.ToString(BuildNumericFormat(BankConstants.MoneyScaleDigits), Culture);

    /// <summary>
    /// Форматирует денежную сумму с наименованием валюты.
    /// </summary>
    /// <param name="value">Денежная сумма.</param>
    /// <returns>Сумма в виде «38 190,45 руб.».</returns>
    public static string Rub(decimal value) => $"{Money(value)} руб.";

    /// <summary>
    /// Форматирует процентное значение по правилам русской типографики.
    /// </summary>
    /// <param name="value">Процентное значение.</param>
    /// <param name="digits">Количество знаков после запятой.</param>
    /// <returns>Процент в виде «50,9 %» либо «50 %» при digits = 0.</returns>
    public static string Percent(decimal value, int digits = BankConstants.PercentDigitsInText) =>
        $"{value.ToString(BuildNumericFormat(digits), Culture)} %";

    /// <summary>
    /// Собирает строковый формат числа с заданным количеством знаков после запятой.
    /// </summary>
    /// <param name="digits">Количество знаков после запятой.</param>
    /// <returns>Строковый формат вида «N2».</returns>
    private static string BuildNumericFormat(int digits) =>
        $"N{digits.ToString(CultureInfo.InvariantCulture)}";
}