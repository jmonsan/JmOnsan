using BotBridge.UI.Services;
using BotBridge.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace BotBridge.UI.Views;

public partial class ProcessesView
{
    public ProcessesView()
    {
        InitializeComponent();

        DataContext =
            ServiceProviderHost.Provider
                .GetRequiredService<ProcessesViewModel>();
    }
}