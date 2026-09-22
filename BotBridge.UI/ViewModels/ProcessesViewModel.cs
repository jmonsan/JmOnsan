using System.Collections.ObjectModel;
<<<<<<< HEAD
=======
using BotBridge.Application.Workers;
>>>>>>> c347f0b (Restore local project)
using BotBridge.Core.Interfaces;
using BotBridge.UI.Commands;
using BotBridge.UI.Models;
using System.Windows;

namespace BotBridge.UI.ViewModels;

public sealed class ProcessesViewModel
    : ViewModelBase
{
    private readonly IProcessService _processService;

    private readonly IProcessDiscoveryService _processDiscoveryService;
<<<<<<< HEAD
    private readonly IAutomationService _automationService;
=======
    private readonly AutomationWorker _automationWorker;
>>>>>>> c347f0b (Restore local project)

    private ProcessItemModel? _selectedProcess;

    private string _processContent =
        string.Empty;

    public ObservableCollection<ProcessItemModel>
        Processes { get; } = [];

    public ProcessItemModel? SelectedProcess
    {
        get => _selectedProcess;
        set
        {
            if (SetProperty(
                    ref _selectedProcess,
                    value))
            {
                _ = LoadProcessContentAsync();
            }
        }
    }

    public string ProcessContent
    {
        get => _processContent;
        set => SetProperty(
            ref _processContent,
            value);
    }

    public RelayCommand RefreshCommand { get; }

    public RelayCommand SaveCommand { get; }

    public RelayCommand RunCommand { get; }

public ProcessesViewModel(
    IProcessService processService,
<<<<<<< HEAD
    IAutomationService automationService,
=======
    AutomationWorker automationWorker,
>>>>>>> c347f0b (Restore local project)
    IProcessDiscoveryService processDiscoveryService)
{
    _processService =
        processService;

<<<<<<< HEAD
    _automationService =
        automationService;
=======
    _automationWorker =
        automationWorker;
>>>>>>> c347f0b (Restore local project)

    _processDiscoveryService =
        processDiscoveryService;

    RefreshCommand =
        new RelayCommand(
            () => _ = LoadProcessesAsync());

    SaveCommand =
        new RelayCommand(
            () => _ = SaveProcessAsync());

    RunCommand =
        new RelayCommand(
            () => _ = RunProcessAsync());

    _ = LoadProcessesAsync();
}


private async Task LoadProcessesAsync()
{
    Processes.Clear();

    var items =
        await _processService
            .GetProcessesAsync();

    foreach (var item in items)
    {
        Processes.Add(
            new ProcessItemModel
            {
                Name = item.Name,
                FullPath = item.FullPath,
                SizeInBytes = item.SizeInBytes
            });
    }
}
    private async Task LoadProcessContentAsync()
    {
        if (SelectedProcess is null)
        {
            return;
        }

        ProcessContent =
            await _processService
                .ReadProcessAsync(
                    SelectedProcess.Name);
    }

    private async Task SaveProcessAsync()
    {
        if (SelectedProcess is null)
        {
            return;
        }

        await _processService
            .SaveProcessAsync(
                SelectedProcess.Name,
                ProcessContent);
    }

    private async Task RunProcessAsync()
    {
        if (SelectedProcess is null)
        {
            return;
        }

<<<<<<< HEAD
        await _automationService
            .ExecuteAsync(
=======
        // Goes through the Run Queue like a scheduled run: it
        // starts immediately when free, or waits (visible on the
        // Run Queue tab) if it conflicts with something running.
        await _automationWorker
            .EnqueueManualRunAsync(
>>>>>>> c347f0b (Restore local project)
                SelectedProcess.Name);
    }
}   