using BotBridge.Core.Interfaces;

namespace BotBridge.Application.Services;

public sealed class FolderInitializer
{
    private readonly IConfigurationService
        _configurationService;

    public FolderInitializer(
        IConfigurationService configurationService)
    {
        _configurationService =
            configurationService;
    }

    public void EnsureFoldersExist()
    {
        var config =
            _configurationService.Current;

        Directory.CreateDirectory(
            config.ConfigFolder);

        Directory.CreateDirectory(
            config.LogsFolder);

        Directory.CreateDirectory(
            config.PackagesFolder);
    }
}