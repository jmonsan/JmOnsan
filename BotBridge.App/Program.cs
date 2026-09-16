using BotBridge.Application;
using BotBridge.Application.Services;
using BotBridge.Application.Workers;
using BotBridge.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BotBridge;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        var services = new ServiceCollection();

        services.AddBotBridge();

        using var provider =
            services.BuildServiceProvider();

        var configurationService =
            provider.GetRequiredService<IConfigurationService>();

        await configurationService.LoadAsync();

        var folderInitializer =
            provider.GetRequiredService<FolderInitializer>();

        folderInitializer.EnsureFoldersExist();

        var worker =
            provider.GetRequiredService<AutomationWorker>();

        using var shutdown =
            new CancellationTokenSource();

        System.Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            shutdown.Cancel();
        };

        System.Console.WriteLine(
            "====================================");

        System.Console.WriteLine(
            "BotBridge Backend Started");

        System.Console.WriteLine(
            "Press CTRL+C to stop");

        System.Console.WriteLine(
            "====================================");

        await worker.StartAsync(
            shutdown.Token);

        try
        {
            await Task.Delay(
                Timeout.Infinite,
                shutdown.Token);
        }
        catch (OperationCanceledException)
        {
        }

        await worker.StopAsync();

        System.Console.WriteLine(
            "BotBridge Backend Stopped");
    }
}
