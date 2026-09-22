using BotBridge.Core.Interfaces;
using BotBridge.Core.Models;

namespace BotBridge.Application.Services;

public sealed class RunQueueService : IRunQueueService
{
    private const int MaxLogLines = 2000;

    private readonly object _gate = new();

    private readonly List<RunQueueEntry> _entries = new();

    public event EventHandler? Changed;

    public int RunningCount
    {
        get
        {
            lock (_gate)
            {
                return _entries.Count(x =>
                    x.Status == RunQueueStatus.Running);
            }
        }
    }

    public int QueuedCount
    {
        get
        {
            lock (_gate)
            {
                return _entries.Count(x =>
                    x.Status == RunQueueStatus.Queued);
            }
        }
    }

    public IReadOnlyList<RunQueueItem> GetSnapshot()
    {
        lock (_gate)
        {
            return _entries
                .Where(x => x.IsVisible)
                .Select(x => x.ToItem())
                .ToList();
        }
    }

    public IReadOnlyList<string> GetRunningProcessNames()
    {
        lock (_gate)
        {
            return _entries
                .Where(x =>
                    x.Status == RunQueueStatus.Running)
                .Select(x => x.ProcessName)
                .ToList();
        }
    }

    public RunQueueItem Enqueue(
        ScheduledProcess scheduledProcess,
        bool isManual,
        int maxConcurrent)
    {
        ArgumentNullException.ThrowIfNull(
            scheduledProcess);

        RunQueueItem snapshot;
        bool visible;

        lock (_gate)
        {
            var now = DateTimeOffset.Now;

            var limit = Math.Max(1, maxConcurrent);

            var running = _entries
                .Where(x =>
                    x.Status == RunQueueStatus.Running)
                .ToList();

            var sameProcessRunning = running.Any(x =>
                string.Equals(
                    x.ProcessName,
                    scheduledProcess.ProcessName,
                    StringComparison.OrdinalIgnoreCase));

            var slotsFull =
                running.Count >= limit;

            var waitingAhead = _entries.Any(x =>
                x.Status == RunQueueStatus.Queued &&
                x.NotBefore <= now);

            var blocked =
                sameProcessRunning ||
                slotsFull ||
                waitingAhead;

            var entry = new RunQueueEntry
            {
                Id = Guid.NewGuid(),
                ProcessName = scheduledProcess.ProcessName,
                Process = scheduledProcess.Process,
                ScheduledTime = scheduledProcess.ScheduledTime,
                EnqueuedAt = now,
                Status = RunQueueStatus.Queued,
                IsManual = isManual,
                IsVisible = blocked,
                NotBefore = DateTimeOffset.MinValue
            };

            entry.AddLog(
                isManual
                    ? "Manual run requested."
                    : $"Scheduled run due at {scheduledProcess.ScheduledTime:yyyy-MM-dd HH:mm:ss}.");

            if (blocked)
            {
                if (sameProcessRunning)
                {
                    entry.AddLog(
                        "Conflict: this process is already running. Added to the Run Queue.");
                }
                else if (slotsFull)
                {
                    entry.AddLog(
                        $"Conflict: {string.Join(", ", running.Select(x => x.ProcessName))} is still running. Added to the Run Queue.");
                }
                else
                {
                    entry.AddLog(
                        "Conflict: earlier executions are waiting. Added to the Run Queue.");
                }
            }

            _entries.Add(entry);

            snapshot = entry.ToItem();
            visible = blocked;
        }

        if (visible)
        {
            RaiseChanged();
        }

        return snapshot;
    }

    public RunQueueItem? TryStartNext(
        int maxConcurrent,
        DateTimeOffset now)
    {
        RunQueueItem? started = null;
        var changed = false;

        lock (_gate)
        {
            var limit = Math.Max(1, maxConcurrent);

            var runningNames =
                new HashSet<string>(
                    _entries
                        .Where(x =>
                            x.Status == RunQueueStatus.Running)
                        .Select(x => x.ProcessName),
                    StringComparer.OrdinalIgnoreCase);

            var runningCount = _entries.Count(x =>
                x.Status == RunQueueStatus.Running);

            if (runningCount < limit)
            {
                var next = _entries.FirstOrDefault(x =>
                    x.Status == RunQueueStatus.Queued &&
                    x.NotBefore <= now &&
                    !runningNames.Contains(x.ProcessName));

                if (next is not null)
                {
                    next.Status = RunQueueStatus.Running;
                    next.Attempts++;

                    if (next.IsVisible)
                    {
                        changed = true;
                    }

                    started = next.ToItem();
                }
            }

            // Anything that is still queued at this point is
            // waiting for a slot, so it belongs on the tab.
            foreach (var waiting in _entries)
            {
                if (waiting.Status == RunQueueStatus.Queued &&
                    !waiting.IsVisible)
                {
                    waiting.IsVisible = true;

                    waiting.AddLog(
                        "Conflict: waiting for a free slot. Added to the Run Queue.");

                    changed = true;
                }
            }
        }

        if (changed)
        {
            RaiseChanged();
        }

        return started;
    }

    public void Requeue(
        Guid id,
        DateTimeOffset notBefore,
        string reason)
    {
        lock (_gate)
        {
            var entry = Find(id);

            if (entry is null)
            {
                return;
            }

            entry.Status = RunQueueStatus.Queued;
            entry.NotBefore = notBefore;
            entry.IsVisible = true;

            entry.AddLog(reason);
        }

        RaiseChanged();
    }

    public void Complete(Guid id)
    {
        var wasVisible = false;

        lock (_gate)
        {
            var entry = Find(id);

            if (entry is null)
            {
                return;
            }

            wasVisible = entry.IsVisible;

            _entries.Remove(entry);
        }

        if (wasVisible)
        {
            RaiseChanged();
        }
    }

    public void Clear()
    {
        var hadVisible = false;

        lock (_gate)
        {
            hadVisible = _entries.Any(x => x.IsVisible);

            _entries.Clear();
        }

        if (hadVisible)
        {
            RaiseChanged();
        }
    }

    public void AppendLog(
        Guid id,
        string line)
    {
        lock (_gate)
        {
            Find(id)?.AddLog(line);
        }
    }

    public string? GetLogText(Guid id)
    {
        lock (_gate)
        {
            var entry = Find(id);

            return entry is null
                ? null
                : string.Join(
                    Environment.NewLine,
                    entry.Logs);
        }
    }

    private RunQueueEntry? Find(Guid id)
    {
        return _entries.FirstOrDefault(x =>
            x.Id == id);
    }

    private void RaiseChanged()
    {
        try
        {
            Changed?.Invoke(
                this,
                EventArgs.Empty);
        }
        catch
        {
            // A misbehaving subscriber must never break the
            // worker that raised the change.
        }
    }

    private sealed class RunQueueEntry
    {
        public Guid Id { get; init; }

        public string ProcessName { get; init; }
            = string.Empty;

        public ProcessConfig Process { get; init; }
            = new();

        public DateTimeOffset ScheduledTime { get; init; }

        public DateTimeOffset EnqueuedAt { get; init; }

        public RunQueueStatus Status { get; set; }

        public bool IsManual { get; init; }

        public bool IsVisible { get; set; }

        public int Attempts { get; set; }

        public DateTimeOffset NotBefore { get; set; }

        public List<string> Logs { get; } = new();

        public void AddLog(string line)
        {
            Logs.Add(
                $"[{DateTime.Now:HH:mm:ss}] {line}");

            if (Logs.Count > MaxLogLines)
            {
                Logs.RemoveRange(
                    0,
                    Logs.Count - MaxLogLines);
            }
        }

        public RunQueueItem ToItem()
        {
            return new RunQueueItem
            {
                Id = Id,
                ProcessName = ProcessName,
                Process = Process,
                ScheduledTime = ScheduledTime,
                EnqueuedAt = EnqueuedAt,
                Status = Status,
                IsManual = IsManual,
                IsVisible = IsVisible,
                Attempts = Attempts
            };
        }
    }
}
