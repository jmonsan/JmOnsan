namespace BotBridge.Core.Interfaces;

public interface IApplicationControlService
{
    bool IsRunning { get; }

    Task StartAsync(
        CancellationToken cancellationToken = default);

    Task StopAsync(
        CancellationToken cancellationToken = default);
}
