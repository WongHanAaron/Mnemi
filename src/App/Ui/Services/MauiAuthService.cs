using Ui.Shared.Services;

namespace Ui.Services;

/// <summary>
/// MAUI implementation of IAuthService.
/// For now, always returns authenticated so the shared UI renders correctly.
/// Future: integrate with WebView cookie sharing or native auth.
/// </summary>
public class MauiAuthService : IAuthService
{
    private readonly AuthState _state = new()
    {
        IsAuthenticated = true,
        DisplayName = "Student",
        Email = "student@mnemi.app"
    };

    public Task<AuthState> GetCurrentAuthStateAsync() => Task.FromResult(_state);

    public Task<AuthResult> LoginAsync(AuthProvider provider)
        => Task.FromResult(AuthResult.Ok(_state));

    public Task<AuthResult> LogoutAsync()
    {
        var unauthenticated = AuthState.Unauthenticated;
        AuthStateChanged?.Invoke(unauthenticated);
        return Task.FromResult(AuthResult.Ok(unauthenticated));
    }

    public event Action<AuthState>? AuthStateChanged;
}
