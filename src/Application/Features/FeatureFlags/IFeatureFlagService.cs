namespace Mnemi.Application.Features.FeatureFlags;

using System.Reflection;

/// <summary>
/// Supported feature flags.
/// </summary>
public enum FeatureFlag
{
    /// <summary>
    /// When enabled, authentication is bypassed so the UI is visible without logging in.
    /// Set via env var E2E_TEST_AUTH_BYPASS=1 or appsettings "FeatureFlags:E2ETestAuthBypass".
    /// </summary>
    [FeatureFlagName("E2E_TEST_AUTH_BYPASS")]
    BypassAuth,
}

/// <summary>
/// Extension methods for FeatureFlag enum.
/// </summary>
public static class FeatureFlagExtensions
{
    /// <summary>
    /// Gets the configuration/environment variable name for the feature flag.
    /// </summary>
    public static string GetConfigName(this FeatureFlag flag)
    {
        var field = typeof(FeatureFlag).GetField(flag.ToString());
        var attribute = field?.GetCustomAttribute<FeatureFlagNameAttribute>();
        
        if (attribute == null)
        {
            throw new InvalidOperationException(
                $"Feature flag '{flag}' is missing [FeatureFlagName] attribute");
        }

        return attribute.Name;
    }
}

/// <summary>
/// Centralized feature flag service. Provides strongly-typed feature checks.
/// </summary>
public interface IFeatureFlagService
{
    /// <summary>
    /// Returns true when auth should be bypassed for end-to-end testing.
    /// </summary>
    bool BypassAuth();

    /// <summary>
    /// Returns true if the requested feature flag is enabled.
    /// </summary>
    bool IsEnabled(FeatureFlag flag);
}
