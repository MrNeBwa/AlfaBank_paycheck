using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace AlfaBank.App.Converters;

public sealed class BooleanToStatusBrushConverter : IValueConverter
{
    private const string SuccessBrushKey = "SuccessBrush";

    private const string DangerBrushKey = "DangerBrush";

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var key = value is true ? SuccessBrushKey : DangerBrushKey;
        var resource = Application.Current?.TryFindResource(key);

        return resource as Brush ?? Brushes.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
}
