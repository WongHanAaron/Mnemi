namespace Mnemi.Application.Features.FeatureFlags;

/// <summary>
/// Marks an enum member with its configuration/environment variable name.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class FeatureFlagNameAttribute : Attribute
{
    /// <summary>
    /// The name of the configuration key or environment variable.
    /// </summary>
    public string Name { get; }

    public FeatureFlagNameAttribute(string name)
    {
        Name = name;
    }
}
