using BotBridge.Core.Models;

namespace BotBridge.Core.Interfaces;

public interface IConfigurationService
{
    /// <summary>
    /// Gets the currently loaded configuration.
    /// </summary>
    AppConfig Current { get; }

    /// <summary>
    /// Loads configuration from disk.
    /// </summary>
    Task<AppConfig> LoadAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves configuration to disk.
    /// </summary>
    Task SaveAsync(
        AppConfig config,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reloads configuration from disk.
    /// </summary>
    Task<AppConfig> ReloadAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates configuration.
    /// Throws ConfigurationException when invalid.
    /// </summary>
    Task ValidateAsync(
        AppConfig config,
        CancellationToken cancellationToken = default);
}