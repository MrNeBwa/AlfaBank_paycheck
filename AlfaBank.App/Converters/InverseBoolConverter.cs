using System.Globalization;
using System.Windows.Data;

namespace AlfaBank.App.Converters;

/// <summary>
/// Преобразователь, возвращающий обратное логическое значение.
/// </summary>
public sealed class InverseBoolConverter : IValueConverter
{
    /// <summary>
    /// Возвращает инвертированное значение флага.
    /// </summary>
    /// <param name="value">Исходное значение.</param>
    /// <param name="targetType">Тип целевого значения.</param>
    /// <param name="parameter">Параметр преобразования.</param>
    /// <param name="culture">Культура преобразования.</param>
    /// <returns>Инвертированное значение.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is not true;

    /// <summary>
    /// Возвращает значение без изменений: обратное преобразование не используется.
    /// </summary>
    /// <param name="value">Исходное значение.</param>
    /// <param name="targetType">Тип целевого значения.</param>
    /// <param name="parameter">Параметр преобразования.</param>
    /// <param name="culture">Культура преобразования.</param>
    /// <returns>Исходное значение.</returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
}