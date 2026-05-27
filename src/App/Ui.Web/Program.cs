using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Mnemi.Application;
using Mnemi.Application.Features.FeatureFlags;
using Mnemi.Application.Home;
using Mnemi.Application.Ports;
using Ui.Shared.Ports;
using Ui.Shared.Services;
using Ui.Web.Components;
using Ui.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddControllers();

// Register Application-layer services
builder.Services.AddApplicationServices();

// Add device-specific services used by the Ui.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();

// WebViewStateService — tracks viewport size via JavaScript interop (Phase 5)
builder.Services.AddScoped<IViewStateService, WebViewStateService>();

// Home dashboard services (stub data provider)
builder.Services.AddScoped<HomeDashboardStubDataProvider>();
builder.Services.AddScoped<IHomeDashboardService, HomeDashboardService>();

// Auth services
builder.Services.AddHttpClient();
builder.Services.AddScoped<WebAuthService>();
builder.Services.AddScoped<IAuthService>(sp => sp.GetRequiredService<WebAuthService>());
builder.Services.AddScoped<ITokenEncryptionService, WebTokenEncryptionService>();
builder.Services.AddScoped<IUserRepository, StubUserRepository>();
builder.Services.AddScoped<IAuthConnectionRepository, StubAuthConnectionRepository>();

// User profile management (Phase 1: stub — replaced by real DB-backed service later)
builder.Services.AddScoped<IUserService, Ui.Shared.Stubs.StubUserService>();

// Document source management (Phase 3: stub — replaced by real DB-backed service later)
builder.Services.AddScoped<IDocumentSourceService, Ui.Shared.Stubs.StubDocumentSourceService>();

// Provider icon service (shared across account components)
builder.Services.AddScoped<Ui.Shared.Components.Account.IProviderIconService, Ui.Shared.Components.Account.DefaultProviderIconService>();

// Feature flags — read from config (env vars / appsettings / CLI)
//   Set env var E2E_TEST_AUTH_BYPASS=1 to bypass auth for E2E tests
builder.Services.AddScoped<IFeatureFlagService, WebFeatureFlagService>();

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
        typeof(Ui.Shared._Imports).Assembly,
        typeof(Ui.Web.Client._Imports).Assembly);

app.Run();

app.Run();
