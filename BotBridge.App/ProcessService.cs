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
    var folder =
        _configurationService.Current
            .PackagesFolder;

    if (!Directory.Exists(folder))
    {
        return [];
    }

    var files =
        Directory.GetFiles(
            folder,
            "*.bat",
            SearchOption.TopDirectoryOnly);

    return files
        .Select(file => new ProcessInfo
        {
            Name =
                Path.GetFileNameWithoutExtension(
                    file),

            FullPath = file,

            SizeInBytes =
                new FileInfo(file).Length
        })
        .ToList();
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

public async Task<string>
    ReadProcessAsync(
        string processName,
        CancellationToken cancellationToken = default)
{
    var folder =
        _configurationService.Current
            .PackagesFolder;

    var file =
        Path.Combine(
            folder,
            $"{processName}.bat");

    if (!File.Exists(file))
    {
        return string.Empty;
    }

return await File.ReadAllTextAsync(
    file,
    cancellationToken);
}

public async Task SaveProcessAsync(
    string processName,
    string content,
    CancellationToken cancellationToken = default)
    {
        var folder =
            _configurationService.Current
                .PackagesFolder;

        var file =
            Path.Combine(
                folder,
                $"{processName}.bat");

await File.WriteAllTextAsync(
    file,
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
            processName.EndsWith(".bat",
                StringComparison.OrdinalIgnoreCase)
                ? processName
                : $"{processName}.bat");
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