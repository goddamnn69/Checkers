using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Checkers.Converters;

public class CheckerColorToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString() switch
        {
            "White" => SolidColorBrush.Parse("#FFFFFF"),
            "Black" => SolidColorBrush.Parse("#2c3e50"),
            _ => Brushes.Transparent
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
