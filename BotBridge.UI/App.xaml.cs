using System;
using System.Windows;
using BotBridge.Application.Services;
using BotBridge.Core.Interfaces;
using BotBridge.UI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BotBridge.UI;

public partial class App : System.Windows.Application
{
    private MainWindow? _mainWindow;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            ServiceProviderHost.Configure();

            var provider = ServiceProviderHost.Provider;

            var configurationService =
                provider.GetRequiredService<IConfigurationService>();

            await configurationService.LoadAsync();

            var folderInitializer =
                provider.GetRequiredService<FolderInitializer>();

            folderInitializer.EnsureFoldersExist();

            _mainWindow =
                provider.GetRequiredService<MainWindow>();

            // Keep the window alive, but don't show it on startup.
            _mainWindow.Hide();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.ToString(),
                "BotBridge startup error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            if (ServiceProviderHost.Provider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        catch
        {
            // Prevent shutdown errors from crashing the application.
        }

        base.OnExit(e);
    }
}