using Microsoft.Extensions.Logging;
using Mnemi.Application;
using Mnemi.Application.Features.FeatureFlags;
using Mnemi.Application.Home;
using Mnemi.Application.Ports;
using Ui.Services;
using Ui.Shared.Components.Account;
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

            // Register Application layer services (IGetPinnedDecks, etc.)
            builder.Services.AddApplicationServices();

            // Add device-specific services used by the Ui.Shared project
            builder.Services.AddSingleton<IFormFactor, FormFactor>();

            // MAUI IViewStateService — detects device idiom for sidebar responsiveness
            builder.Services.AddScoped<IViewStateService, MauiViewStateService>();

            // MAUI IAuthService — stub that returns authenticated (shared components require it)
            builder.Services.AddScoped<IAuthService, MauiAuthService>();

            // MAUI IUserService — shared stub (shared Account page requires it)
            builder.Services.AddScoped<IUserService, Ui.Shared.Stubs.StubUserService>();

            // MAUI IDocumentSourceService — shared stub (shared Account page requires it)
            builder.Services.AddScoped<IDocumentSourceService, Ui.Shared.Stubs.StubDocumentSourceService>();

            // MAUI IHomeDashboardService — stub dashboard data (shared Home page requires it)
            builder.Services.AddScoped<IHomeDashboardService, MauiHomeDashboardService>();

            // Provider icon service (shared across account components)
            builder.Services.AddScoped<IProviderIconService, DefaultProviderIconService>();

            // Feature flags
            builder.Services.AddScoped<IFeatureFlagService, MauiFeatureFlagService>();

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
