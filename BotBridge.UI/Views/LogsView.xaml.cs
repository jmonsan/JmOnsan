using System.Windows;
using System.Windows.Input;
using BotBridge.UI.Services;
using BotBridge.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;


namespace BotBridge.UI.Views;

public partial class LogsView : UserControl
{
    private readonly LogsViewModel _viewModel;

    public LogsView(
        LogsViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;

        DataContext = _viewModel;
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