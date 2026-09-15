using BotBridge.UI.Services;
using BotBridge.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;


namespace BotBridge.UI.Views;

public partial class SchedulesView : UserControl
{
    public SchedulesView(
        SchedulesViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
    }
}