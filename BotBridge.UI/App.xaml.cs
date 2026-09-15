using System.Windows;
using BotBridge.UI.Services;

namespace BotBridge.UI;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(
        StartupEventArgs e)
    {
        base.OnStartup(e);

        ServiceProviderHost.Configure();
        var window = new MainWindow();

        window.Hide();
    }
}