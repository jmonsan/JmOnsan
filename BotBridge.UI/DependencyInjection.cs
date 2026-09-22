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
<<<<<<< HEAD
        services.AddSingleton<LogsViewModel>();
=======
        services.AddSingleton<RunQueueViewModel>();
        services.AddSingleton<LogsViewModel>();
        services.AddSingleton<ExecutionLogsViewModel>();
>>>>>>> c347f0b (Restore local project)

        // Main window
        services.AddSingleton<MainWindow>();

        // Views
        services.AddSingleton<DashboardView>();
        services.AddSingleton<ProcessesView>();
        services.AddSingleton<SchedulesView>();
<<<<<<< HEAD
        services.AddSingleton<LogsView>();
=======
        services.AddSingleton<RunQueueView>();
        services.AddSingleton<LogsView>();
        services.AddSingleton<ExecutionLogsView>();
>>>>>>> c347f0b (Restore local project)

        return services;
    }
}
