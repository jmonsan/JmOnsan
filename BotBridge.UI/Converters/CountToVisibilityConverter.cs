using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BotBridge.UI.Converters;

/// <summary>
/// Visible when a bound collection is empty (Count == 0),
/// Collapsed otherwise. Used to show "empty state" placeholders
/// without changing any ViewModel.
/// </summary>
public sealed class CountToVisibilityConverter : IValueConverter
{
    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        var count = value switch
        {
            int intCount => intCount,
            ICollection collection => collection.Count,
            IEnumerable enumerable => CountEnumerable(enumerable),
            _ => 0
        };

        return count == 0
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    private static int CountEnumerable(IEnumerable enumerable)
    {
        var count = 0;

        foreach (var _ in enumerable)
        {
            count++;
        }

        return count;
    }
}
