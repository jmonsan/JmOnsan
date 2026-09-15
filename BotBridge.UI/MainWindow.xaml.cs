using System.ComponentModel;
using System.Windows;
using BotBridge.UI.Tray;
using BotBridge.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using BotBridge.UI.Services;
using BotBridge.Core.Interfaces;
using BotBridge.Core.Models;

namespace BotBridge.UI;

public partial class MainWindow : Window
{
    private readonly TrayIconService _trayIconService;

    private bool _isExiting;

    public MainWindow()
    {
        InitializeComponent();


            var statusService =
            ServiceProviderHost.Provider
                .GetRequiredService<IStatusService>();

        statusService.Update(status =>
        {
            status.State =
                WorkerState.Running;

            status.CurrentProcess =
                "Test Process";

            status.NextScheduledRun =
                DateTimeOffset.Now.AddMinutes(5);

            status.LastExecutionTime =
                DateTimeOffset.Now;
        });




        DataContext =
            ServiceProviderHost.Provider
                .GetRequiredService<MainViewModel>();

        _trayIconService =
            new TrayIconService();

        _trayIconService.OpenRequested +=
            (_, _) => ShowWindow();

        _trayIconService.ExitRequested +=
            (_, _) => ExitApplication();
    }

    protected override void OnClosing(
        CancelEventArgs e)
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

        WindowState =
            WindowState.Normal;

        Activate();
    }

    private void ExitApplication()
    {
        _isExiting = true;

        _trayIconService.Dispose();

        System.Windows.Application
            .Current
            .Shutdown();
    }
}