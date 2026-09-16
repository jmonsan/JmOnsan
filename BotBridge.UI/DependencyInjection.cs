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
        // No path parameter: config, logs, execution history and
        // status all live at the fixed Desktop\BotBridge locations
        // (see BotBridge.Core.DesktopPaths).
        services.AddBotBridge();

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
