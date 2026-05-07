using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using Mnemi.Application;
using Mnemi.Application.Home;
using Mnemi.Application.Ports;
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

        // ---- Authentication Setup ----
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            options.Cookie.Name = "Mnemi.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.LoginPath = "/login";
            options.LogoutPath = "/api/auth/logout";
            options.ExpireTimeSpan = TimeSpan.FromDays(14);
            options.SlidingExpiration = true;
            options.Events.OnRedirectToLogin = context =>
            {
                // For API requests, return 401 instead of redirecting
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                }
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            };
        })
        .AddGoogle(options =>
        {
            options.ClientId = builder.Configuration["Authentication:Google:ClientId"]
                ?? "PLACEHOLDER_GOOGLE_CLIENT_ID";
            options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]
                ?? "PLACEHOLDER_GOOGLE_CLIENT_SECRET";
            options.SaveTokens = true;
            options.Scope.Add("email");
            options.Scope.Add("profile");
            options.Scope.Add("https://www.googleapis.com/auth/drive.readonly");
        });

        // GitHub authentication can be added when the package is available:
        // .AddGitHub(options => { ... })

        // Register OAuth token encryption service
        builder.Services.AddScoped<ITokenEncryptionService, WebTokenEncryptionService>();

        // Register data protection for token encryption
        builder.Services.AddDataProtection();

        // Register HttpClient for auth state checks
        builder.Services.AddHttpClient();

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

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");

        app.Run();
    }
}
