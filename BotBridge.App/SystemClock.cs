using BotBridge.Core.Interfaces;

namespace BotBridge.Infrastructure.System;

public sealed class SystemClock : IClock
{
    public DateTimeOffset Now =>
        DateTimeOffset.Now;

    public DateTime Today =>
        DateTime.Today;

    public DateTimeOffset UtcNow =>
        DateTimeOffset.UtcNow;
}