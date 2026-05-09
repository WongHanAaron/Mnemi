using Mnemi.Application.Features.FeatureFlags;

namespace Ui.Services;

/// <summary>
/// MAUI feature flag service. Reads flags from environment variables.
/// </summary>
public class MauiFeatureFlagService : IFeatureFlagService
{
    public bool BypassAuth() => IsEnabled(FeatureFlag.BypassAuth);

    public bool IsEnabled(FeatureFlag flag) => IsEnabled(flag.GetConfigName());

    private bool IsEnabled(string flag)
    {
        var value = Environment.GetEnvironmentVariable(flag);
        return !string.IsNullOrEmpty(value);
    }
}
