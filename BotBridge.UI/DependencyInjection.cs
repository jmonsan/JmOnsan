using System.IO;
using BotBridge.Application;
using BotBridge.UI.ViewModels;
using BotBridge.UI.Views;
using Microsoft.Extensions.DependencyInjection;

namespace BotBridge.UI;

public static class DependencyInjection
{
    public static IServiceCollection AddBotBridgeUi(
        this IServiceCollection services)
    {
        var configPath = Path.Combine(
            AppContext.BaseDirectory,
            "Config",
            "appsettings.json");

        services.AddBotBridge(configPath);

        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<DashboardViewModel>();
        services.AddSingleton<ProcessesViewModel>();
        services.AddSingleton<SchedulesViewModel>();
        services.AddSingleton<LogsViewModel>();

        // Main window
        services.AddSingleton<MainWindow>();

        // Views
        services.AddSingleton<DashboardView>();
        services.AddSingleton<ProcessesView>();
        services.AddSingleton<SchedulesView>();
        services.AddSingleton<LogsView>();

        return services;
    }
}