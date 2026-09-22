using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using BotBridge.UI.ViewModels;

namespace BotBridge.UI.Views;

public partial class ExecutionLogsView : UserControl
{
    private readonly ExecutionLogsViewModel _viewModel;

    private readonly DispatcherTimer _refreshTimer;

    public ExecutionLogsView(
        ExecutionLogsViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;

        DataContext = _viewModel;

        _refreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5)
        };

        _refreshTimer.Tick += RefreshTimer_Tick;

        Loaded += ExecutionLogsView_Loaded;
        Unloaded += ExecutionLogsView_Unloaded;
    }

    private void ExecutionLogsView_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        _refreshTimer.Start();
    }

    private void ExecutionLogsView_Unloaded(
        object sender,
        RoutedEventArgs e)
    {
        _refreshTimer.Stop();
    }

    private void RefreshTimer_Tick(
        object? sender,
        EventArgs e)
    {
        _ = _viewModel.RefreshAsync();
    }

    private async void LogFiles_MouseDoubleClick(
        object sender,
        MouseButtonEventArgs e)
    {
        if (_viewModel.SelectedLogFile is null)
        {
            return;
        }

        var content =
            await _viewModel.ReadLogFileAsync(
                _viewModel.SelectedLogFile.FullPath);

        var detailWindow =
            new LogDetailWindow(
                _viewModel.SelectedLogFile.FileName,
                content)
            {
                Owner = Window.GetWindow(this)
            };

        detailWindow.Show();
    }
}
