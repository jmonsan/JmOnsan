using BotBridge.Application.Services;
using BotBridge.Application.Workers;
using BotBridge.Core.Interfaces;
using BotBridge.Infrastructure.Configuration;
using BotBridge.Infrastructure.Logging;
using BotBridge.Infrastructure.Persistence;
using BotBridge.Infrastructure.System;
using Microsoft.Extensions.DependencyInjection;

namespace BotBridge.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddBotBridge(
        this IServiceCollection services,
        string configurationPath)
    {
        // Infrastructure

        services.AddSingleton<IClock, SystemClock>();

        services.AddSingleton<IConfigurationService>(
            _ => new JsonConfigurationService(
                configurationPath));

        services.AddSingleton<IExecutionHistoryStore>(
            _ =>
            {
                var historyFile =
                    Path.Combine(
                        AppContext.BaseDirectory,
                        "config",
                        "execution-history.json");

                return new ExecutionHistoryStore(
                    historyFile);
            });

        services.AddSingleton<ILoggerService>(
            _ =>
            {
                var logsFolder =
                    Path.Combine(
                        AppContext.BaseDirectory,
                        "logs");

                return new FileLogger(
                    logsFolder);
            });

        // Application Services

        services.AddSingleton<IStatusService,
            StatusService>();

        services.AddSingleton<IProcessService,
            ProcessService>();

        services.AddSingleton<IScheduleService,
            ScheduleService>();

        services.AddSingleton<IAutomationService,
            AutomationService>();

        services.AddSingleton<
            IProcessDiscoveryService,
            ProcessDiscoveryService>();

        services.AddSingleton<
            IApplicationControlService,
            ApplicationControlService>();
        // Worker

        services.AddSingleton<AutomationWorker>();
        services.AddSingleton<FolderInitializer>();


        return services;
    }
}