using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BotBridge.UI.Converters;

/// <summary>
/// True (HasError) -> danger red, False -> success green.
/// Used to color a ViewModelBase.StatusMessage line without
/// needing a dedicated brush property on every ViewModel.
/// </summary>
public sealed class BoolToStatusBrushConverter : IValueConverter
{
    private static readonly Brush DangerBrush = Brush("#DC2626");
    private static readonly Brush SuccessBrush = Brush("#059669");

    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        var isError = value is bool flag && flag;

        return isError
            ? DangerBrush
            : SuccessBrush;
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    private static Brush Brush(string hex)
    {
        var brush =
            (Brush)new BrushConverter().ConvertFromString(hex)!;

        brush.Freeze();

        return brush;
    }
}
