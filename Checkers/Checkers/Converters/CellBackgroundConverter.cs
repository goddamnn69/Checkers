using Avalonia;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Checkers.Converters;

public class CellBackgroundConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {

        if (values.Any(v => v == AvaloniaProperty.UnsetValue))
            return Brushes.Red;
        if (values.Count < 3) return Brushes.Yellow;

        bool isDark = values[0] is true;
        bool isSelected = values[1] is true;
        bool isTarget = values[2] is true;

        if (isTarget) return SolidColorBrush.Parse("#2ecc71");
        if (isSelected) return SolidColorBrush.Parse("#3498db");
        return isDark ? SolidColorBrush.Parse("#95a5a6") : SolidColorBrush.Parse("#ecf0f1");
    }
}
