namespace BotBridge.UI.Models;

public sealed class ProcessItemModel
{
    public string Name { get; set; } = string.Empty;

    public string FullPath { get; set; } = string.Empty;

    public long SizeInBytes { get; set; }
}