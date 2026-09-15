using BotBridge.Core.Interfaces;
using BotBridge.UI.ViewModels;
using BotBridge.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using BotBridge.Infrastructure.Configuration;
namespace BotBridge.UI;

public static class DependencyInjection
{
    public static IServiceCollection AddBotBridgeUi(
        this IServiceCollection services)
    {
        // Backend services

        services.AddSingleton<IStatusService,
            StatusService>();

        services.AddSingleton<
            IApplicationControlService,
            ApplicationControlService>();

        // ViewModels

        services.AddSingleton<MainViewModel>();

        services.AddTransient<DashboardViewModel>();

        services.AddTransient<ProcessesViewModel>();

        services.AddTransient<SchedulesViewModel>();

        services.AddTransient<LogsViewModel>();

        services.AddTransient<SettingsViewModel>();
        services.AddSingleton<IProcessService,
            ProcessService>();

        services.AddSingleton<IProcessDiscoveryService,
            ProcessDiscoveryService>();

        services.AddSingleton<IAutomationService,
            AutomationService>();
            
services.AddSingleton<IConfigurationService>(
    _ =>
    {
        var configPath =
            @"C:\Users\JeironeMarcoOnsan\Desktop\BotBridge\Config\appsettings.json";

        var service =
            new JsonConfigurationService(
                configPath);

        service.LoadAsync()
            .GetAwaiter()
            .GetResult();

        return service;
    });


        return services;
    }
}

