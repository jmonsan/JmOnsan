using BotBridge.Core.Interfaces;
using BotBridge.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace BotBridge.UI;

public static class DependencyInjection
{
    public static IServiceCollection AddBotBridgeUi(
        this IServiceCollection services)
    {
        services.AddSingleton<MainViewModel>();

        services.AddTransient<DashboardViewModel>();

        services.AddTransient<ProcessesViewModel>();

        services.AddTransient<SchedulesViewModel>();

        services.AddTransient<LogsViewModel>();

        services.AddTransient<SettingsViewModel>();

        return services;
    }
}
    