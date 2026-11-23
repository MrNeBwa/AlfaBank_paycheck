using System.Globalization;
using System.Windows.Data;

namespace AlfaBank.App.Converters;

public sealed class DateOnlyConverter : IValueConverter
{
    public const string DateFormat = "dd.MM.yyyy";

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is DateOnly date ? date.ToString(DateFormat, CultureInfo.InvariantCulture) : string.Empty;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("Преобразование даты в модель представления не поддерживается.");
}
