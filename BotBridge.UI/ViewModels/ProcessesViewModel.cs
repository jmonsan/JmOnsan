using System.Collections.ObjectModel;
using BotBridge.Core.Interfaces;
using BotBridge.UI.Commands;
using BotBridge.UI.Models;

namespace BotBridge.UI.ViewModels;

public sealed class ProcessesViewModel
    : ViewModelBase
{
    private readonly IProcessService _processService;

    private readonly IAutomationService _automationService;

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
        IAutomationService automationService)
    {
        _processService =
            processService;

        _automationService =
            automationService;

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

        await _automationService
            .ExecuteAsync(
                SelectedProcess.Name);
    }
}   