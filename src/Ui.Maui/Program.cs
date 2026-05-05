using Microsoft.Extensions.DependencyInjection;
using Mnemi.Application.Home;
using Mnemi.Ui.Shared.Ports;
using Mnemi.Ui.Maui.Services;

namespace Mnemi.Ui.Maui;

public class Program
{
    public static IServiceProvider? Services { get; private set; }

    public static void Main(string[] args)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<IHomeDashboardService, HomeDashboardService>();

        // Register viewport/viewstate service (MAUI window size detection)
        serviceCollection.AddScoped<IViewStateService, MauiViewStateService>();

        Services = serviceCollection.BuildServiceProvider();
    }
}
