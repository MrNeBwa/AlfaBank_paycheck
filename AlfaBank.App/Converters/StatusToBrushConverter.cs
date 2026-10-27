using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using AlfaBank.Core.Models.Enums;

namespace AlfaBank.App.Converters;

/// <summary>
/// Подбирает цвет для статуса заявки, кредита, плана графика или платежа.
/// Нейтральные статусы остаются серыми, благоприятные — зелёными, неблагоприятные — красными.
/// </summary>
public sealed class StatusToBrushConverter : IValueConverter
{
    /// <summary>
    /// Имя ресурса цвета в словаре темы оформления.
    /// </summary>
    private const string SuccessBrushKey = "SuccessBrush";

    /// <summary>
    /// Имя ресурса отрицательного цвета в словаре темы оформления.
    /// </summary>
    private const string DangerBrushKey = "DangerBrush";

    /// <summary>
    /// Имя ресурса нейтрального цвета в словаре темы оформления.
    /// </summary>
    private const string NeutralBrushKey = "NeutralBrush";

    /// <summary>
    /// Возвращает цвет, соответствующий статусу.
    /// </summary>
    /// <param name="value">Значение статуса из доменной модели.</param>
    /// <param name="targetType">Тип целевого значения.</param>
    /// <param name="parameter">Параметр преобразования.</param>
    /// <param name="culture">Культура преобразования.</param>
    /// <returns>Кисть, соответствующая статусу.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value switch
    {
        CreditApplicationStatus.New => FindResource(NeutralBrushKey),
        CreditApplicationStatus.Approved => FindResource(SuccessBrushKey),
        CreditApplicationStatus.Rejected => FindResource(DangerBrushKey),

        CreditStatus.Active => FindResource(NeutralBrushKey),
        CreditStatus.Closed => FindResource(SuccessBrushKey),

        ScheduleItemStatus.Paid => FindResource(SuccessBrushKey),
        ScheduleItemStatus.Planned => FindResource(NeutralBrushKey),
        ScheduleItemStatus.Overdue => FindResource(DangerBrushKey),

        PaymentStatus.Registered => FindResource(SuccessBrushKey),
        PaymentStatus.Cancelled => FindResource(DangerBrushKey),

        _ => FindResource(NeutralBrushKey)
    };

    /// <summary>
    /// Возвращает значение без изменений: обратное преобразование не используется.
    /// </summary>
    /// <param name="value">Исходное значение.</param>
    /// <param name="targetType">Тип целевого значения.</param>
    /// <param name="parameter">Параметр преобразования.</param>
    /// <param name="culture">Культура преобразования.</param>
    /// <returns>Исходное значение.</returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;

    /// <summary>
    /// Находит кисть в общих ресурсах приложения, чтобы палитра оставалась единой.
    /// </summary>
    /// <param name="key">Ключ ресурса.</param>
    /// <returns>Кисть из словаря темы оформления.</returns>
    private static Brush FindResource(string key)
    {
        var resource = Application.Current?.TryFindResource(key);

        return resource as Brush ?? Brushes.Gray;
    }
}
