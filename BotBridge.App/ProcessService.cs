using BotBridge.Core.Interfaces;
using BotBridge.Core.Models;

namespace BotBridge.Application.Services;

public sealed class ProcessService : IProcessService
{
    private readonly IConfigurationService _configurationService;

    public ProcessService(
        IConfigurationService configurationService)
    {
        _configurationService = configurationService;
    }

    public async Task<IReadOnlyCollection<ProcessInfo>>
        GetProcessesAsync(
            CancellationToken cancellationToken = default)
    {
        var packagesFolder =
            _configurationService.Current.PackagesFolder;

        Directory.CreateDirectory(packagesFolder);

        var processes = Directory
            .GetFiles(packagesFolder, "*.bat")
            .Select(CreateProcessInfo)
            .OrderBy(x => x.Name)
            .ToList()
            .AsReadOnly();

        return await Task.FromResult(processes);
    }

    public async Task<ProcessInfo?> GetProcessAsync(
        string processName,
        CancellationToken cancellationToken = default)
    {
        var path =
            await GetProcessPathAsync(
                processName,
                cancellationToken);

        if (!File.Exists(path))
        {
            return null;
        }

        return CreateProcessInfo(path);
    }

    public async Task<string> ReadProcessAsync(
        string processName,
        CancellationToken cancellationToken = default)
    {
        var path =
            await GetProcessPathAsync(
                processName,
                cancellationToken);

        if (!File.Exists(path))
        {
            throw new FileNotFoundException(
                "Process file not found.",
                path);
        }

        return await File.ReadAllTextAsync(
            path,
            cancellationToken);
    }

    public async Task SaveProcessAsync(
        string processName,
        string content,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        var path =
            await GetProcessPathAsync(
                processName,
                cancellationToken);

        await File.WriteAllTextAsync(
            path,
            content,
            cancellationToken);
    }

    public async Task CreateProcessAsync(
        string processName,
        string content,
        CancellationToken cancellationToken = default)
    {
        var path =
            await GetProcessPathAsync(
                processName,
                cancellationToken);

        if (File.Exists(path))
        {
            throw new InvalidOperationException(
                $"Process '{processName}' already exists.");
        }

        await File.WriteAllTextAsync(
            path,
            content,
            cancellationToken);
    }

    public async Task DeleteProcessAsync(
        string processName,
        CancellationToken cancellationToken = default)
    {
        var path =
            await GetProcessPathAsync(
                processName,
                cancellationToken);

        if (!File.Exists(path))
        {
            return;
        }

        File.Delete(path);

        await Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(
        string processName,
        CancellationToken cancellationToken = default)
    {
        var path =
            await GetProcessPathAsync(
                processName,
                cancellationToken);

        return File.Exists(path);
    }

    public async Task<string> GetProcessPathAsync(
        string processName,
        CancellationToken cancellationToken = default)
    {
        await ValidateProcessAsync(
            processName,
            cancellationToken);

        var packagesFolder =
            _configurationService.Current.PackagesFolder;

        return Path.Combine(
            packagesFolder,
            processName);
    }

    public Task ValidateProcessAsync(
        string processName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(processName))
        {
            throw new ArgumentException(
                "Process name is required.",
                nameof(processName));
        }

        processName = processName.Trim();

        if (Path.GetFileName(processName) != processName)
        {
            throw new ArgumentException(
                "Invalid process name.");
        }

        if (!processName.EndsWith(
                ".bat",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Process must be a .bat file.");
        }

        return Task.CompletedTask;
    }

    private static ProcessInfo CreateProcessInfo(
        string filePath)
    {
        var fileInfo = new FileInfo(filePath);

        return new ProcessInfo
        {
            Name = fileInfo.Name,
            FullPath = fileInfo.FullName,
            SizeInBytes = fileInfo.Length,
            CreatedAt = fileInfo.CreationTimeUtc,
            LastModifiedAt = fileInfo.LastWriteTimeUtc,
            Exists = fileInfo.Exists
        };
    }
}