using Microsoft.Extensions.Logging;
using Mnemi.Application.Home;
using Ui.Services;
using Ui.Shared.Ports;
using Ui.Shared.Services;

namespace Ui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            // Add device-specific services used by the Ui.Shared project
            builder.Services.AddSingleton<IFormFactor, FormFactor>();

            // MAUI IViewStateService — detects device idiom for sidebar responsiveness
            builder.Services.AddScoped<IViewStateService, MauiViewStateService>();

            // MAUI IAuthService — stub that returns authenticated (shared components require it)
            builder.Services.AddScoped<IAuthService, MauiAuthService>();

            // MAUI IHomeDashboardService — stub dashboard data (shared Home page requires it)
            builder.Services.AddScoped<IHomeDashboardService, MauiHomeDashboardService>();

            // Feature flags
            builder.Services.AddScoped<IAuthBypassService, MauiAuthBypassService>();

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
