using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using AlfaBank.Core.Models.Enums;

namespace AlfaBank.App.Converters;

public sealed class StatusToBrushConverter : IValueConverter
{
    private const string SuccessBrushKey = "SuccessBrush";

    private const string DangerBrushKey = "DangerBrush";

    private const string NeutralBrushKey = "NeutralBrush";

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

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;

    private static Brush FindResource(string key)
    {
        var resource = Application.Current?.TryFindResource(key);

        return resource as Brush ?? Brushes.Gray;
    }
}
