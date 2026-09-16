using System.Windows;

namespace BotBridge.UI.Views;

public partial class LogDetailWindow : Window
{
    public LogDetailWindow(
        string fileName,
        string content)
    {
        InitializeComponent();

        Title = fileName;

        ContentTextBox.Text = content;
    }

}
