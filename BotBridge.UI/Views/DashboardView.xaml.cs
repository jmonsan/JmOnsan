using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using BotBridge.UI.ViewModels;

namespace BotBridge.UI.Views;

public partial class DashboardView : UserControl
{
    private readonly DispatcherTimer _refreshTimer;

    public DashboardView(
        DashboardViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;

        _refreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(30)
        };

        _refreshTimer.Tick += RefreshTimer_Tick;

        Loaded += DashboardView_Loaded;
        Unloaded += DashboardView_Unloaded;
    }

    private void DashboardView_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        _refreshTimer.Start();
    }

    private void DashboardView_Unloaded(
        object sender,
        RoutedEventArgs e)
    {
        _refreshTimer.Stop();
    }

    private void RefreshTimer_Tick(
        object? sender,
        EventArgs e)
    {
        if (DataContext is DashboardViewModel viewModel)
        {
            viewModel.Refresh();
        }
    }
}