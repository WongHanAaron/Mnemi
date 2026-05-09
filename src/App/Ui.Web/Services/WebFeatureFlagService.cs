using Ui.Shared.Services;

namespace Ui.Web.Services;

/// <summary>
/// Internal mapping of feature flags to configuration key names.
/// </summary>
internal static class FeatureFlagNames
{
    public const string BypassAuth = "E2E_TEST_AUTH_BYPASS";
}

/// <summary>
/// Server-side feature flag service backed by ASP.NET Core IConfiguration.
/// Flags can be set via environment variables, appsettings.json, or CLI args.
/// 
/// Each flag is checked against configuration keys in order:
///   1. Direct env var name (e.g. "E2E_TEST_AUTH_BYPASS")
///   2. Structured config path (e.g. "FeatureFlags:BypassAuth")
/// </summary>
public class WebFeatureFlagService : IFeatureFlagService
{
    private readonly IConfiguration _configuration;

    public WebFeatureFlagService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool BypassAuth() => IsEnabled(FeatureFlag.BypassAuth);

    public bool IsEnabled(FeatureFlag flag) => IsEnabled(GetConfigName(flag));

    private static string GetConfigName(FeatureFlag flag) => flag switch
    {
        FeatureFlag.BypassAuth => FeatureFlagNames.BypassAuth,
        _ => throw new ArgumentOutOfRangeException(nameof(flag), flag, null)
    };

    private bool IsEnabled(string flag)
    {
        // 1. Check as a top-level config key (supports E2E_TEST_AUTH_BYPASS env var directly)
        var direct = _configuration[flag];
        if (IsTruthy(direct))
            return true;

        // 2. Check under FeatureFlags: prefix (supports appsettings, env var FeatureFlags__Xxx)
        var structured = _configuration[$"FeatureFlags:{flag}"];
        return IsTruthy(structured);
    }

    private static bool IsTruthy(string? value) =>
        !string.IsNullOrEmpty(value)
        && (value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase));
}
