using BotBridge.Core.Interfaces;

namespace BotBridge.Application.Services;

public sealed class ProcessDiscoveryService
    : IProcessDiscoveryService
{
    private readonly IConfigurationService
        _configurationService;

    public ProcessDiscoveryService(
        IConfigurationService configurationService)
    {
        _configurationService =
            configurationService;
    }

    public Task<IReadOnlyCollection<string>>
        GetAvailableProcessesAsync(
            CancellationToken cancellationToken = default)
    {
        var folder =
            _configurationService
                .Current
                .PackagesFolder;

        if (!Directory.Exists(folder))
        {
            return Task.FromResult<
                IReadOnlyCollection<string>>(
                Array.Empty<string>());
        }

        var files =
            Directory.GetFiles(
                folder,
                "*.bat",
                SearchOption.TopDirectoryOnly)
            .Select(
                Path.GetFileNameWithoutExtension)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Cast<string>()
            .ToList();

        return Task.FromResult<
            IReadOnlyCollection<string>>(
            files);
    }
}
