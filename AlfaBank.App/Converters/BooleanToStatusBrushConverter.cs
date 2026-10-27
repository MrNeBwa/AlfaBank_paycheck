using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace AlfaBank.App.Converters;

/// <summary>
/// Преобразователь, окрашивающий значение по логическому признаку:
/// true — положительный результат (зелёный), false — отрицательный (красный).
/// </summary>
public sealed class BooleanToStatusBrushConverter : IValueConverter
{
    /// <summary>
    /// Имя ресурса цвета положительного результата в словаре темы оформления.
    /// </summary>
    private const string SuccessBrushKey = "SuccessBrush";

    /// <summary>
    /// Имя ресурса цвета отрицательного результата в словаре темы оформления.
    /// </summary>
    private const string DangerBrushKey = "DangerBrush";

    /// <summary>
    /// Возвращает цвет, соответствующий значению признака.
    /// </summary>
    /// <param name="value">Логическое значение.</param>
    /// <param name="targetType">Тип целевого значения.</param>
    /// <param name="parameter">Параметр преобразования.</param>
    /// <param name="culture">Культура преобразования.</param>
    /// <returns>Зелёная кисть для true, красная — для false.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var key = value is true ? SuccessBrushKey : DangerBrushKey;
        var resource = Application.Current?.TryFindResource(key);

        return resource as Brush ?? Brushes.Gray;
    }

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
