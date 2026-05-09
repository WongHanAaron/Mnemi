namespace Ui.Shared.Services;

/// <summary>
/// Supported feature flags.
/// </summary>
public enum FeatureFlag
{
    BypassAuth,
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
