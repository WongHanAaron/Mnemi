using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Mnemi.Application.Home;
using Mnemi.Ui.Web.Services;

namespace Mnemi.Ui.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped<HomeDashboardStubDataProvider>();
        builder.Services.AddScoped<IHomeDashboardService, HomeDashboardService>();

        await builder.Build().RunAsync();
    }
}
