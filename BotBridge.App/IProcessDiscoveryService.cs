namespace BotBridge.Core.Interfaces;

public interface IProcessDiscoveryService
{
    Task<IReadOnlyCollection<string>>
        GetAvailableProcessesAsync(
            CancellationToken cancellationToken = default);
}