# BotBridge

BotBridge automates the scheduling and execution of UiPath processes packaged as `.bat` files, entirely offline, with no dependency on Chrome, the internet, or Windows Task Scheduler.

It started as a Chrome Extension + C# Native Messaging Host. It's being migrated into a standalone Windows application (`BotBridge.exe`) with its own built-in scheduler, so it can run continuously as a background app with no browser involved at all.

## What it does

- Lists `.bat` process files from a local `Packages` folder
- Lets you view and edit their contents
- Runs any process on demand
- Schedules a process to run **once**, **daily**, or **weekly**, with per-process enable/disable
- Enforces a **per-process max runtime** — a stuck automation is killed (including any child processes it spawned) instead of running forever
- Keeps a local run history (last run time, last result, last error) per process
- Does all of the above with **zero external dependencies** — no cloud services, no web server, no internet connection required

## Current status

| Piece | Status |
|---|---|
| `BotBridge.Core` — scheduling, automation, file, config, and logging engine | ✅ Implemented |
| `BotBridge.Console` — minimal console harness for manually running/testing the engine | ✅ Implemented |
| Unit tests (scheduling math, config load/save) | ✅ Implemented |
| WPF desktop app + system tray icon | 🔜 Not started yet |
| Chrome Extension + Native Messaging Host | 🗄️ Legacy — being retired in favor of the standalone app |

## Architecture

```
BotBridge/
├── src/
│   ├── BotBridge.Core/          Class library — all real logic lives here
│   │   ├── Models/               AppConfig, ScheduleInfo, TaskResult, WorkerStatus, ...
│   │   ├── Interfaces/           IFileService, IAutomationService, IScheduleService, ...
│   │   ├── Services/             Concrete implementations of the interfaces above
│   │   └── Workers/               AutomationWorker — the background run loop
│   │
│   └── BotBridge.Console/       Minimal console entry point for manual testing
│                                  (temporary — will be replaced by the WPF app)
│
├── config/
│   └── schedules.json            Per-process schedule data (see format below)
│
├── logs/
│   └── botbridge-YYYY-MM-DD.log  Daily rotated log file
│
└── tests/
    └── BotBridge.Core.Tests/     Unit tests for scheduling math and configuration
```

`BotBridge.Core` has no UI code in it at all (no WPF, no console I/O, no `MessageBox`). A future WPF frontend — and the current console harness — are just thin callers of `AutomationWorker`, so the same engine can be driven by either.

### Why there's no Windows Task Scheduler involved

Earlier versions used `schtasks.exe` to create real Windows Scheduled Tasks. That's reliable but external to the app — schedule state can drift between what's shown in the UI and what Task Scheduler actually has registered, and every change requires shelling out to `schtasks.exe`.

Since BotBridge is meant to run as an always-on background app anyway, `AutomationWorker` schedules itself: it computes the single nearest due task across all enabled schedules, arms one timer for exactly that moment, runs it, and recomputes. A periodic safety-net recheck and a Windows sleep/wake hook cover system clock changes and the timer missing its window during sleep. Enable/Disable, Create/Update, and run history are just fields in `schedules.json` — a single source of truth, edited only by this app.

**Trade-off:** this means BotBridge itself must be running for schedules to fire — unlike Task Scheduler, which works even if BotBridge is closed. Since the intended deployment is an always-on background app, this trade-off was made deliberately.

## `schedules.json` format

```json
{
  "AWIR_UAT.bat": {
    "scheduleType": "daily",
    "dateTime": "2026-09-14T01:20",
    "enabled": true,
    "maxRuntimeMinutes": 30,
    "lastRunAt": "2026-09-13T01:20:03",
    "lastResult": "success",
    "lastError": null
  }
}
```

| Field | Meaning |
|---|---|
| `scheduleType` | `"once"`, `"daily"`, or `"weekly"` |
| `dateTime` | Local date/time (`yyyy-MM-ddTHH:mm`). For `daily`/`weekly`, only the time (and day-of-week for weekly) is used — the date is just a reference point. |
| `enabled` | If `false`, this schedule is skipped entirely. |
| `maxRuntimeMinutes` | Per-process timeout override. If omitted, falls back to the app-wide default. |
| `lastRunAt` / `lastResult` / `lastError` | Written automatically after every run attempt; used to prevent double-firing within the same scheduled slot. |

## Getting started

**Prerequisites:** [.NET 8 SDK](https://dotnet.microsoft.com/download), Windows, VS Code (with the C# Dev Kit extension) or any editor — no Visual Studio required.

```bash
# Run all unit tests
dotnet test tests/BotBridge.Core.Tests

# Run the engine via the console harness (manual testing, Ctrl+C to stop)
dotnet run --project src/BotBridge.Console

# Build everything
dotnet build BotBridge.sln
```

The console harness prints its status (current state, current task, next scheduled task/time) every 10 seconds so you can watch it work without any UI.

## Roadmap

1. WPF frontend — desktop window covering the same operations the old Chrome popup did (process list, editor, Run Now, scheduling), consuming `BotBridge.Core` directly.
2. System tray icon so the app can run minimized in the background instead of keeping a window open.
3. Retire the Chrome Extension and Native Messaging Host entirely once the WPF app covers its functionality.
