using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BotBridge.UI.Converters;

/// <summary>
/// Maps a WorkerState.ToString() value to a status-pill color.
/// ConverterParameter "Background" or "Foreground" selects which
/// half of the pair to return. Unknown values fall back to a
/// neutral slate color, so this never throws for any state value.
/// </summary>
public sealed class StatusToBrushConverter : IValueConverter
{
    private static readonly Brush SuccessBg = Brush("#ECFDF5");
    private static readonly Brush SuccessFg = Brush("#059669");

    private static readonly Brush InfoBg = Brush("#EFF6FF");
    private static readonly Brush InfoFg = Brush("#2563EB");

    private static readonly Brush WarningBg = Brush("#FFFBEB");
    private static readonly Brush WarningFg = Brush("#D97706");

    private static readonly Brush DangerBg = Brush("#FEF2F2");
    private static readonly Brush DangerFg = Brush("#DC2626");

    private static readonly Brush NeutralBg = Brush("#F1F5F9");
    private static readonly Brush NeutralFg = Brush("#475569");

    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        var wantsForeground =
            string.Equals(
                parameter as string,
                "Foreground",
                StringComparison.OrdinalIgnoreCase);

        var state = value as string ?? string.Empty;

        return state switch
        {
            "Running" or "Waiting" =>
                wantsForeground ? SuccessFg : SuccessBg,

            "Executing" or "Starting" =>
                wantsForeground ? InfoFg : InfoBg,

            "Stopping" =>
                wantsForeground ? WarningFg : WarningBg,

            "Error" =>
                wantsForeground ? DangerFg : DangerBg,

            _ =>
                wantsForeground ? NeutralFg : NeutralBg
        };
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
