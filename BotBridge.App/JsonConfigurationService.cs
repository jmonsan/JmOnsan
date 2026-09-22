using System.Text.Json;
using BotBridge.Core;
using BotBridge.Core.Exceptions;
using BotBridge.Core.Interfaces;
using BotBridge.Core.Models;

namespace BotBridge.Infrastructure.Configuration;

public sealed class JsonConfigurationService
    : IConfigurationService
{
    private readonly string _configFilePath;

    private readonly JsonSerializerOptions _jsonOptions =
        new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

    public AppConfig Current { get; private set; }
        = new();

    /// <summary>
    /// Always reads/writes the fixed Desktop config file
    /// (%USERPROFILE%\Desktop\BotBridge\Config\appsettings.json).
    /// </summary>
    public JsonConfigurationService()
        : this(DesktopPaths.AppSettingsFile)
    {
    }

    public JsonConfigurationService(string configFilePath)
    {
        _configFilePath = configFilePath;
    }

    public async Task<AppConfig> LoadAsync(
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_configFilePath))
        {
            Current = CreateDefaultConfiguration();

            await SaveAsync(
                Current,
                cancellationToken);

            return Current;
        }

        try
        {
            var json = await File.ReadAllTextAsync(
                _configFilePath,
                cancellationToken);

            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ConfigurationException(
                    "Configuration file is empty.");
            }

            var config =
                JsonSerializer.Deserialize<AppConfig>(
                    json,
                    _jsonOptions);

            if (config is null)
            {
                throw new ConfigurationException(
                    "Unable to deserialize configuration.");
            }

            // Folder locations are fixed/hardcoded and not
            // user-configurable — always force them to the
            // Desktop location regardless of what (if
            // anything) is present in the JSON file.
            ApplyFixedFolders(config);

            await ValidateAsync(
                config,
                cancellationToken);

            Current = config;

            return Current;
        }
        catch (JsonException ex)
        {
            throw new ConfigurationException(
                "Invalid configuration JSON.",
                ex);
        }
    }

    public async Task<AppConfig> ReloadAsync(
        CancellationToken cancellationToken = default)
    {
        return await LoadAsync(cancellationToken);
    }

    public async Task SaveAsync(
        AppConfig config,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(config);

        ApplyFixedFolders(config);

        await ValidateAsync(
            config,
            cancellationToken);

        var directory =
            Path.GetDirectoryName(_configFilePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json =
            JsonSerializer.Serialize(
                config,
                _jsonOptions);

        await File.WriteAllTextAsync(
            _configFilePath,
            json,
            cancellationToken);

        Current = config;
    }

    /// <summary>
    /// Adds a new process/schedule entry, or updates the
    /// existing entry with a matching process name, then
    /// persists the configuration to disk.
    /// </summary>
    public async Task AddOrUpdateScheduleAsync(
        ProcessConfig process,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(process);

        var updated = Current;

        var existing =
            updated.Processes.FirstOrDefault(x =>
                string.Equals(
                    x.Name,
                    process.Name,
                    StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
        {
            updated.Processes.Remove(existing);
        }

        updated.Processes.Add(process);

        await SaveAsync(
            updated,
            cancellationToken);
    }

    public Task ValidateAsync(
        AppConfig config,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (string.IsNullOrWhiteSpace(
                config.PackagesFolder))
        {
            throw new ConfigurationException(
                "PackagesFolder is required.");
        }

        if (string.IsNullOrWhiteSpace(
                config.ConfigFolder))
        {
            throw new ConfigurationException(
                "ConfigFolder is required.");
        }

        if (string.IsNullOrWhiteSpace(
                config.LogsFolder))
        {
            throw new ConfigurationException(
                "LogsFolder is required.");
        }

        if (config.MaxConcurrentTasks <= 0)
        {
            throw new ConfigurationException(
                "MaxConcurrentTasks must be greater than zero.");
        }

<<<<<<< HEAD
        if (config.ProcessTimeoutMinutes <= 0)
        {
            throw new ConfigurationException(
                "ProcessTimeoutMinutes must be greater than zero.");
        }

=======
>>>>>>> c347f0b (Restore local project)
        foreach (var process in config.Processes)
        {
            ValidateProcess(process);
        }

        return Task.CompletedTask;
    }

    private static void ValidateProcess(
        ProcessConfig process)
    {
        if (string.IsNullOrWhiteSpace(process.Name))
        {
            throw new ConfigurationException(
                "Process name is required.");
        }

        if (string.IsNullOrWhiteSpace(process.FileName))
        {
            throw new ConfigurationException(
                $"FileName is required for process '{process.Name}'.");
        }

        if (!process.FileName.EndsWith(
                ".bat",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ConfigurationException(
                $"Process '{process.Name}' must reference a .bat file.");
        }

        if (process.Schedule is null)
        {
            throw new ConfigurationException(
                $"Schedule is required for process '{process.Name}'.");
        }

        if (process.Schedule.StartDateTime ==
            default)
        {
            throw new ConfigurationException(
                $"StartDateTime is required for process '{process.Name}'.");
        }
    }

    private static void ApplyFixedFolders(AppConfig config)
    {
        config.PackagesFolder = DesktopPaths.PackagesFolder;
        config.ConfigFolder = DesktopPaths.ConfigFolder;
        config.LogsFolder = DesktopPaths.LogsFolder;
    }

    private static AppConfig CreateDefaultConfiguration()
    {
        var config = new AppConfig
        {
            Enabled = true,

            MaxConcurrentTasks = 1,

<<<<<<< HEAD
            ProcessTimeoutMinutes = 60,

=======
>>>>>>> c347f0b (Restore local project)
            RunMissedSchedules = true
        };

        ApplyFixedFolders(config);

        return config;
    }
}