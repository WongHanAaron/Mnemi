using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Ui.Shared.Ports;
using Ui.Shared.Services;
using Ui.Web.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add device-specific services used by the Ui.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();

// Stub IViewStateService — used by AppSidebar (replaced in Phase 5)
builder.Services.AddScoped<IViewStateService, StubViewStateService>();

await builder.Build().RunAsync();
