using Microsoft.Extensions.DependencyInjection;
using Mnemi.Application.Home;
using Mnemi.Ui.Maui.Services;

namespace Mnemi.Ui.Maui;

public class Program
{
    public static IServiceProvider? Services { get; private set; }

    public static void Main(string[] args)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<IHomeDashboardService, HomeDashboardService>();
        Services = serviceCollection.BuildServiceProvider();
    }
}
