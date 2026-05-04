namespace Mnemi.Ui.Shared.Services;

/// <summary>
/// Represents the current authentication state of the user.
/// </summary>
public class AuthState
{
    /// <summary>
    /// Whether the user is currently authenticated.
    /// </summary>
    public bool IsAuthenticated { get; init; }

    /// <summary>
    /// The user's email address.
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// The user's display name.
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// URL to the user's avatar image.
    /// </summary>
    public string? AvatarUrl { get; init; }

    /// <summary>
    /// Unique identifier for the user.
    /// </summary>
    public string? UserId { get; init; }

    /// <summary>
    /// The OAuth provider used for authentication (e.g., "Google", "GitHub").
    /// </summary>
    public string? Provider { get; init; }

    /// <summary>
    /// Returns an unauthenticated state.
    /// </summary>
    public static AuthState Unauthenticated => new() { IsAuthenticated = false };
}

/// <summary>
/// Available OAuth providers for authentication.
/// </summary>
public enum AuthProvider
{
    Google,
    GitHub
}

/// <summary>
/// Result of an authentication operation.
/// </summary>
public class AuthResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public AuthState? State { get; init; }

    public static AuthResult Ok(AuthState state) => new() { Success = true, State = state };
    public static AuthResult Fail(string error) => new() { Success = false, ErrorMessage = error };
}
