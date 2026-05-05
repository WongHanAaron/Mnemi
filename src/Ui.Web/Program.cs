using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using Mnemi.Application;
using Mnemi.Application.Home;
using Mnemi.Ui.Shared.Ports;
using Mnemi.Ui.Shared.Services;
using Mnemi.Ui.Web.Services;

namespace Mnemi.Ui.Web;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Add services to the container
        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor(options =>
        {
            options.DetailedErrors = true;
        });
        builder.Services.AddControllers();

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

        var app = builder.Build();

        // Configure the HTTP request pipeline
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();

        app.MapControllers();
        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");

        app.Run();
    }
}
