using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Mnemi.Ui.Components.Services;
using Mnemi.Ui.Web.Services;
using Mnemi.Application.Services;
using Mnemi.Application.Services.Implementations;
using BlazorBlueprint.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<Mnemi.Ui.Components.Pages.Home>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Blazor Blueprint
builder.Services.AddBlazorBlueprintComponents();

// Shared UI Services (Host Specific)
builder.Services.AddScoped<IViewStateService, WebViewStateService>();
builder.Services.AddScoped<IDeviceInfoService, WebDeviceInfoService>();

// Application Services (Mock for now)
builder.Services.AddScoped<IUserService, MockUserService>();
builder.Services.AddScoped<IDeckService, MockDeckService>();
builder.Services.AddScoped<IStatisticsService, MockStatisticsService>();

await builder.Build().RunAsync();
