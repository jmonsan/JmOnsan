using BotBridge.Application.Services;
using BotBridge.Application.Workers;
using BotBridge.Core;
using BotBridge.Core.Interfaces;
using BotBridge.Infrastructure.Configuration;
using BotBridge.Infrastructure.Logging;
using BotBridge.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace BotBridge.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registers all BotBridge services. Every file BotBridge
    /// reads or writes (appsettings.json, execution-history.json,
    /// status.json, log files, .bat packages) lives at the fixed
    /// Desktop locations defined in <see cref="DesktopPaths"/> —
    /// there is no configurable path anywhere in this graph.
    /// </summary>
    public static IServiceCollection AddBotBridge(
        this IServiceCollection services)
    {
        // =========================
        // Infrastructure
        // =========================

        services.AddSingleton<IConfigurationService>(
            _ => new JsonConfigurationService(
                DesktopPaths.AppSettingsFile));

        services.AddSingleton<ILoggerService>(
            _ => new FileLogger(
                DesktopPaths.LogsFolder));

        services.AddSingleton<IExecutionHistoryStore>(
            _ => new ExecutionHistoryStore(
                DesktopPaths.ExecutionHistoryFile));

        services.AddSingleton<IStatusService>(
            _ => new StatusService(
                DesktopPaths.StatusFile));

        // =========================
        // Application Services
        // =========================

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

        // =========================
        // Worker
        // =========================

        services.AddSingleton<AutomationWorker>();

        services.AddSingleton<FolderInitializer>();

        return services;
    }
}
