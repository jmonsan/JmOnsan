namespace BotBridge.Core.Interfaces;

public interface IClock
{
    DateTimeOffset Now { get; }

    DateTime Today { get; }

    DateTimeOffset UtcNow { get; }
}