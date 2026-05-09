using Ui.Shared.Services;

namespace Ui.Services;

/// <summary>
/// Internal mapping of feature flags to environment variable names.
/// </summary>
internal static class FeatureFlagNames
{
    public const string BypassAuth = "E2E_TEST_AUTH_BYPASS";
}

/// <summary>
/// MAUI feature flag service. Reads flags from environment variables.
/// </summary>
public class MauiFeatureFlagService : IFeatureFlagService
{
    public bool BypassAuth() => IsEnabled(FeatureFlag.BypassAuth);

    public bool IsEnabled(FeatureFlag flag) => IsEnabled(GetConfigName(flag));

    private static string GetConfigName(FeatureFlag flag) => flag switch
    {
        FeatureFlag.BypassAuth => FeatureFlagNames.BypassAuth,
        _ => throw new ArgumentOutOfRangeException(nameof(flag), flag, null)
    };

    private bool IsEnabled(string flag)
    {
        var value = Environment.GetEnvironmentVariable(flag);
        return !string.IsNullOrEmpty(value);
    }
}
