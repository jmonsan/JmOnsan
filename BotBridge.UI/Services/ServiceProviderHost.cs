using Microsoft.Extensions.DependencyInjection;

namespace BotBridge.UI.Services;

public static class ServiceProviderHost
{
    public static IServiceProvider Provider
    {
        get;
        private set;
    } = null!;

    public static void Configure()
    {
        var services =
            new ServiceCollection();

        services.AddBotBridgeUi();

        Provider =
            services.BuildServiceProvider();
    }
}