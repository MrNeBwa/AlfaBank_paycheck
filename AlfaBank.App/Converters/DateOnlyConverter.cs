using System.Globalization;
using System.Windows.Data;

namespace AlfaBank.App.Converters;

/// <summary>
/// Преобразует дату в формате «день.месяц.год» для отображения в интерфейсе.
/// </summary>
public sealed class DateOnlyConverter : IValueConverter
{
    /// <summary>
    /// Формат даты, используемый в интерфейсе программы.
    /// </summary>
    public const string DateFormat = "dd.MM.yyyy";

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is DateOnly date ? date.ToString(DateFormat, CultureInfo.InvariantCulture) : string.Empty;

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("Преобразование даты в модель представления не поддерживается.");
}
