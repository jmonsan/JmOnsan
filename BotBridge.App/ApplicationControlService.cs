using BotBridge.Core.Models;
using BotBridge.Core.Interfaces;
using BotBridge.Application.Services;

namespace BotBridge.Application.Services;

public sealed class ApplicationControlService
    : IApplicationControlService
{
    private readonly IStatusService _statusService;

    public bool IsRunning =>
        _statusService.Current.State !=
        WorkerState.Stopped;

    public ApplicationControlService(
        IStatusService statusService)
    {
        _statusService = statusService;
    }

    public Task StartAsync(
        CancellationToken cancellationToken = default)
    {
        _statusService.Update(status =>
        {
            status.State =
                WorkerState.Running;
        });

        return Task.CompletedTask;
    }

    public Task StopAsync(
        CancellationToken cancellationToken = default)
    {
        _statusService.Update(status =>
        {
            status.State =
                WorkerState.Stopped;
        });

        return Task.CompletedTask;
    }
}