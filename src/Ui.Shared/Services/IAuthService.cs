namespace Mnemi.Ui.Shared.Services;

/// <summary>
/// Service interface for authentication operations.
/// Shared across Web and MAUI platforms.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Gets the current authentication state.
    /// </summary>
    Task<AuthState> GetCurrentAuthStateAsync();

    /// <summary>
    /// Initiates login with the specified OAuth provider.
    /// </summary>
    Task<AuthResult> LoginAsync(AuthProvider provider);

    /// <summary>
    /// Logs the user out.
    /// </summary>
    Task<AuthResult> LogoutAsync();

    /// <summary>
    /// Event raised when authentication state changes.
    /// </summary>
    event Action<AuthState>? AuthStateChanged;
}
