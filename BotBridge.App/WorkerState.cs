namespace BotBridge.Core.Models;

public enum WorkerState
{
    Stopped = 0,

    Starting = 1,

    Running = 2,

    Waiting = 3,

    Executing = 4,

    Stopping = 5,

    Error = 6
}