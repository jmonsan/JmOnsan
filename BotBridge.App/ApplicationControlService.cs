using BotBridge.Application.Workers;
using BotBridge.Core.Interfaces;
using BotBridge.Core.Models;

namespace BotBridge.Application.Services;

public sealed class ApplicationControlService
    : IApplicationControlService
{
    private readonly IStatusService _statusService;

    private readonly AutomationWorker _worker;

    public bool IsRunning =>
        _worker.IsRunning;

    public ApplicationControlService(
        IStatusService statusService,
        AutomationWorker worker)
    {
        _statusService =
            statusService;

        _worker =
            worker;
    }

    public async Task StartAsync(
        CancellationToken cancellationToken = default)
    {
        if (_worker.IsRunning)
        {
            return;
        }

        await _worker.StartAsync(
            cancellationToken);
    }

    public async Task StopAsync(
        CancellationToken cancellationToken = default)
    {
        if (!_worker.IsRunning)
        {
            return;
        }

        await _worker.StopAsync();
    }
}