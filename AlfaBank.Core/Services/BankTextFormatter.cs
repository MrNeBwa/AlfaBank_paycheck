using System.Globalization;

namespace AlfaBank.Core.Services;

public static class BankTextFormatter
{
    public static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("ru-RU");

    public static string Money(decimal value) =>
        value.ToString(BuildNumericFormat(BankConstants.MoneyScaleDigits), Culture);

    public static string Rub(decimal value) => $"{Money(value)} руб.";

    public static string Percent(decimal value, int digits = BankConstants.PercentDigitsInText) =>
        $"{value.ToString(BuildNumericFormat(digits), Culture)} %";

    private static string BuildNumericFormat(int digits) =>
        $"N{digits.ToString(CultureInfo.InvariantCulture)}";
}