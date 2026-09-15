using BotBridge.Core.Models;

namespace BotBridge.Core.Interfaces;

public interface IStatusService
{
    BackendStatus Current { get; }

    event EventHandler<BackendStatus>? StatusChanged;

    void Update(Action<BackendStatus> updateAction);

    BackendStatus GetSnapshot();

    void Reset();
}