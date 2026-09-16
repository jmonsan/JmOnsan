using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BotBridge.UI.Converters;

/// <summary>
/// Collapsed when the bound value is null (or an empty string),
/// Visible otherwise. "Invert=True" as ConverterParameter flips
/// the result — used for "no selection yet" placeholders.
/// </summary>
public sealed class NullToVisibilityConverter : IValueConverter
{
    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        var isNullOrEmpty =
            value is null ||
            (value is string text &&
             string.IsNullOrWhiteSpace(text));

        var invert =
            string.Equals(
                parameter as string,
                "Invert",
                StringComparison.OrdinalIgnoreCase);

        if (invert)
        {
            isNullOrEmpty = !isNullOrEmpty;
        }

        return isNullOrEmpty
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
