using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Hardcodet.Wpf.TaskbarNotification;

namespace BotBridge.UI.Tray;

public sealed class TrayIconService : IDisposable
{
    private readonly TaskbarIcon _trayIcon;
    private bool _disposed;

    public event EventHandler? OpenRequested;
    public event EventHandler? ExitRequested;

    public TrayIconService()
    {
        var iconPath = Path.Combine(
            AppContext.BaseDirectory,
            "Assets",
            "BotBridge.ico");

        _trayIcon = new TaskbarIcon
        {
            ToolTipText = "BotBridge",
            Icon = File.Exists(iconPath)
                ? new Icon(iconPath)
                : SystemIcons.Application
        };

        _trayIcon.ContextMenu = CreateContextMenu();

        _trayIcon.TrayLeftMouseDown +=
            OnTrayIconClicked;
    }

    private ContextMenu CreateContextMenu()
    {
        var menu = new ContextMenu();

        var openItem = new MenuItem
        {
            Header = "Open BotBridge"
        };

        openItem.Click += (_, _) =>
        {
            OpenRequested?.Invoke(
                this,
                EventArgs.Empty);
        };

        var exitItem = new MenuItem
        {
            Header = "Exit"
        };

        exitItem.Click += (_, _) =>
        {
            ExitRequested?.Invoke(
                this,
                EventArgs.Empty);
        };

        menu.Items.Add(openItem);
        menu.Items.Add(new Separator());
        menu.Items.Add(exitItem);

        return menu;
    }

    private void OnTrayIconClicked(
        object sender,
        RoutedEventArgs e)
    {
        OpenRequested?.Invoke(
            this,
            EventArgs.Empty);
    }

    public void ShowBalloon(
        string title,
        string message)
    {
        if (_disposed)
        {
            return;
        }

        _trayIcon.ShowBalloonTip(
            title,
            message,
            BalloonIcon.Info);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _trayIcon.TrayLeftMouseDown -=
            OnTrayIconClicked;

        _trayIcon.Dispose();
    }
}