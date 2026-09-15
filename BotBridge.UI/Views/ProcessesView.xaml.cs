using BotBridge.UI.Services;
using BotBridge.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;


namespace BotBridge.UI.Views;

public partial class ProcessesView : UserControl
{
    public ProcessesView(
        ProcessesViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
    }
}