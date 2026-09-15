using System.ComponentModel;
using System.Windows;
using BotBridge.UI.Services;
using BotBridge.UI.Tray;
using BotBridge.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace BotBridge.UI;

public partial class MainWindow : Window
{
    private readonly TrayIconService _trayIconService;
    private bool _isExiting;

    public MainWindow()
    {
        InitializeComponent();

        DataContext =
            ServiceProviderHost.Provider
                .GetRequiredService<MainViewModel>();

        _trayIconService = new TrayIconService();

        _trayIconService.OpenRequested +=
            (_, _) => ShowWindow();

        _trayIconService.ExitRequested +=
            (_, _) => ExitApplication();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_isExiting)
        {
            e.Cancel = true;
            Hide();
            return;
        }

        base.OnClosing(e);
    }

    private void ShowWindow()
    {
        Show();

        ShowInTaskbar = true;

        if (WindowState == WindowState.Minimized)
        {
            WindowState = WindowState.Normal;
        }

        Activate();
        Topmost = true;
        Topmost = false;
        Focus();
    }

    private void ExitApplication()
    {
        if (_isExiting)
        {
            return;
        }

        _isExiting = true;

        _trayIconService.Dispose();

        System.Windows.Application.Current.Shutdown();
    }
}