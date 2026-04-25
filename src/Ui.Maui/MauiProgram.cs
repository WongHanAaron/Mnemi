using Microsoft.AspNetCore.Components.WebView.Maui;
using Mnemi.Ui.Components.Services;
using Mnemi.Ui.Maui.Services;
using Mnemi.Application.Services;
using Mnemi.Application.Services.Implementations;
using BlazorBlueprint.Components;

namespace Mnemi.Ui.Maui;

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

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif

        // Blazor Blueprint
        builder.Services.AddBlazorBlueprintComponents();

        // Shared UI Services (Host Specific)
        builder.Services.AddScoped<IViewStateService, MauiViewStateService>();
        builder.Services.AddScoped<IDeviceInfoService, MauiDeviceInfoService>();

        // Application Services (Mock for now)
        builder.Services.AddScoped<IUserService, MockUserService>();
        builder.Services.AddScoped<IDeckService, MockDeckService>();
        builder.Services.AddScoped<IStatisticsService, MockStatisticsService>();

        return builder.Build();
    }
}
