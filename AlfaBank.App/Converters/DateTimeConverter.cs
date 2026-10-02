using System.Globalization;
using System.Windows.Data;

namespace AlfaBank.App.Converters;

/// <summary>
/// Преобразует дату и время в формате «день.месяц.год час:минута».
/// </summary>
public sealed class DateTimeConverter : IValueConverter
{
    /// <summary>
    /// Формат даты и времени, используемый в интерфейсе программы.
    /// </summary>
    public const string DateTimeFormat = "dd.MM.yyyy HH:mm";

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is DateTime date ? date.ToString(DateTimeFormat, CultureInfo.InvariantCulture) : string.Empty;

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("Преобразование даты в модель представления не поддерживается.");
}
