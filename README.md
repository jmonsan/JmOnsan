# BotBridge

A lightweight Windows background service + WPF dashboard for scheduling, running, and monitoring `.bat`-based automations (including UiPath Robot processes).

BotBridge lives in the system tray, watches a fixed folder on your Desktop for `.bat` files, runs them on a schedule (or on demand), and gives you a live dashboard, a code editor for the scripts themselves, and a browsable log history — all with zero manual configuration.

---

## Features

- **Background tray app** — runs hidden at all times; closing the window just hides it to the tray. Only "Exit" from the tray menu actually terminates the process.
- **Live dashboard** — status, current task, running-process count, next scheduled run, and last execution result, auto-refreshing every 30 seconds.
- **Process management** — browse, edit, save, and run the `.bat` files in your `Packages` folder from a built-in code editor.
- **Scheduling** — create or update `Once` / `Daily` / `Weekly` schedules per process from a dedicated tab; schedules persist to the config file automatically.
- **Log browser** — browse log files by day, double-click to open the full contents in a popup viewer.
- **Force-kill protection** — every run has a max-runtime timeout; if it's exceeded, BotBridge kills the entire process tree (the `.bat`'s shell, the UiPath Robot process it spawned, and anything *that* spawned).
- **Zero configuration** — no Settings screen. All folders and JSON files live at fixed, predictable paths under your Desktop.

---

## Fixed folder structure

BotBridge always reads and writes at these exact paths — nothing is user-configurable, and there's no Settings screen:

```
%USERPROFILE%\Desktop\BotBridge\
├── Packages\              .bat files representing automatable processes
├── Config\
│   ├── appsettings.json   app settings + registered processes/schedules ("memory")
│   ├── execution-history.json   persisted run history (duplicate-execution prevention)
│   └── status.json        persisted worker/dashboard status snapshot
└── Logs\                  one log file per day
```

All three files in `Config\` are created automatically on first run if they don't already exist. `appsettings.json` is re-read every 30 seconds while the worker is running, so schedule/process edits take effect without a restart.

---

## Project structure

```
BotBridge/
├── BotBridge.App/     Backend: config, scheduling, process execution, logging, persistence
│   ├── Interfaces/    Service contracts (IConfigurationService, IScheduleService, ...)
│   ├── Models/         AppConfig, ProcessConfig, ScheduleDefinition, BackendStatus, ...
│   ├── Services/       JsonConfigurationService, ScheduleService, AutomationService, ...
│   └── Program.cs      Standalone console entry point (optional headless mode)
│
└── BotBridge.UI/      Frontend: WPF tray app + dashboard
    ├── Views/          DashboardView, ProcessesView, SchedulesView, LogsView, LogDetailWindow
    ├── ViewModels/      MVVM view models for each screen
    ├── Themes/          Design system — Colors.xaml, Typography.xaml, Controls.xaml
    ├── Converters/      Small, reusable value converters (empty states, status colors)
    ├── Tray/            System tray icon + menu
    └── App.xaml.cs      Startup: loads config, auto-starts the worker, hides to tray
```

`BotBridge.UI` references `BotBridge.App` and hosts the same worker in-process, so the desktop app is fully self-contained — you don't need to run `BotBridge.App` separately unless you want a headless/service deployment.

---

## Getting started

**Requirements:** .NET 8 SDK, Windows (WPF).

```bash
git clone <this-repo>
cd BotBridge
dotnet build
```

Run the desktop app:

```bash
dotnet run --project BotBridge.UI
```

Or open `BotBridge.sln` in Visual Studio and set `BotBridge.UI` as the startup project.

On first launch, BotBridge creates `Packages`, `Config`, and `Logs` under `%USERPROFILE%\Desktop\BotBridge\` automatically. Drop a `.bat` file into `Packages` and it will show up in the **Processes** tab.

---

## Tech stack

- **.NET 8**, C#
- **WPF** with a hand-rolled MVVM setup (no third-party MVVM framework)
- **System.Text.Json** for all persistence — no database
- Custom lightweight design system (colors, typography, buttons, cards, inputs) under `BotBridge.UI/Themes/` — no external UI component library

---

## Design system

The UI follows a small, consistent design system rather than ad-hoc styling per screen:

| Aspect | Approach |
|---|---|
| Color | Indigo brand accent + a slate neutral scale, defined once in `Themes/Colors.xaml` |
| Typography | A handful of named text styles (`TextDisplay`, `TextTitle`, `TextBody`, `TextLabel`, `TextMetric`) instead of inline font sizes |
| Buttons | `PrimaryButtonStyle`, `SecondaryButtonStyle`, `SidebarNavButtonStyle`, `IconButtonStyle` — consistent radius, hover/pressed/disabled states |
| Cards | A single `CardStyle` (rounded corners + subtle drop shadow) used across every screen |
| Inputs | Shared `TextBoxStyle`, `ComboBoxStyle`, `CheckBoxStyle` with a focus-state accent border |
| Empty states | `CountToVisibilityConverter` / `NullToVisibilityConverter` drive placeholder content with no ViewModel changes required |

---

## License

Internal / private project — add a license here if you intend to open-source it.
