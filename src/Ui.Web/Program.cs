using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Mnemi.Application;
using Mnemi.Application.Home;
using Mnemi.Ui.Shared.Ports;
using Mnemi.Ui.Shared.Services;
using Mnemi.Ui.Web.Services;

namespace Mnemi.Ui.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        // Register application-layer abstractions (no host-specific implementations)
        builder.Services.AddApplicationServices();

        // Register auth services
        builder.Services.AddScoped<WebAuthService>();
        builder.Services.AddScoped<IAuthService>(sp => sp.GetRequiredService<WebAuthService>());

        // Register viewport/viewstate service (browser resize detection)
        builder.Services.AddScoped<IViewStateService, WebViewStateService>();

        // Register web-specific service implementations
        builder.Services.AddScoped<HomeDashboardStubDataProvider>();
        builder.Services.AddScoped<IHomeDashboardService, HomeDashboardService>();

        await builder.Build().RunAsync();
    }
}
