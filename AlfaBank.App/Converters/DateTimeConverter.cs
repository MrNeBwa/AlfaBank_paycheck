using System.Globalization;
using System.Windows.Data;

namespace AlfaBank.App.Converters;

public sealed class DateTimeConverter : IValueConverter
{
    public const string DateTimeFormat = "dd.MM.yyyy HH:mm";

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is DateTime date ? date.ToString(DateTimeFormat, CultureInfo.InvariantCulture) : string.Empty;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("Преобразование даты в модель представления не поддерживается.");
}
