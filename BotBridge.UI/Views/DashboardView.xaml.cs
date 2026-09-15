using BotBridge.UI.Services;
using BotBridge.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace BotBridge.UI.Views;

public partial class DashboardView
{
    public DashboardView()
    {
        InitializeComponent();

        DataContext =
            ServiceProviderHost.Provider
                .GetRequiredService<DashboardViewModel>();
    }
}