namespace Mnemi.Application.Ports;

/// <summary>
/// Shared validation constants for user-facing inputs.
/// Used by both UI components (client-side) and service implementations (server-side)
/// to ensure consistent validation rules.
/// </summary>
public static class ValidationConstants
{
    public const int MaxDisplayNameLength = 100;
    public const int MaxSourceNameLength = 100;
}
