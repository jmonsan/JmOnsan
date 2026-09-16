using BotBridge.Core;

namespace BotBridge.Application.Services;

public sealed class FolderInitializer
{
    public void EnsureFoldersExist()
    {
        Directory.CreateDirectory(
            DesktopPaths.ConfigFolder);

        Directory.CreateDirectory(
            DesktopPaths.LogsFolder);

        Directory.CreateDirectory(
            DesktopPaths.PackagesFolder);
    }
}
