using System.Windows;
using System.Windows.Controls;
using BotBridge.UI.Models;
using BotBridge.UI.ViewModels;

namespace BotBridge.UI.Views;

public partial class RunQueueView : UserControl
{
    private readonly RunQueueViewModel _viewModel;

    public RunQueueView(
        RunQueueViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;

        DataContext = _viewModel;

        Loaded += RunQueueView_Loaded;
        Unloaded += RunQueueView_Unloaded;
    }

    private void RunQueueView_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        _viewModel.Start();
    }

    private void RunQueueView_Unloaded(
        object sender,
        RoutedEventArgs e)
    {
        _viewModel.Stop();
    }

    private void ViewLogs_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: RunQueueItemModel item })
        {
            return;
        }

        var content =
            _viewModel.GetLogText(item.Id);

        var detailWindow =
            new LogDetailWindow(
                $"{item.ProcessName} — Run Queue Log",
                content)
            {
                Owner = Window.GetWindow(this)
            };

        detailWindow.Show();
    }
}
