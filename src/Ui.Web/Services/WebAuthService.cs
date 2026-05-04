using Microsoft.AspNetCore.Components;
using Mnemi.Ui.Shared.Services;

namespace Mnemi.Ui.Web.Services;

/// <summary>
/// Web-specific implementation of IAuthService for Blazor WebAssembly.
/// Manages auth state via browser localStorage and redirects for OAuth.
/// </summary>
public class WebAuthService : IAuthService, IDisposable
{
    private readonly NavigationManager _navigation;
    private AuthState _currentState = AuthState.Unauthenticated;

    public WebAuthService(NavigationManager navigation)
    {
        _navigation = navigation;
    }

    public event Action<AuthState>? AuthStateChanged;

    public Task<AuthState> GetCurrentAuthStateAsync()
    {
        return Task.FromResult(_currentState);
    }

    public Task<AuthResult> LoginAsync(AuthProvider provider)
    {
        var providerName = provider switch
        {
            AuthProvider.Google => "google",
            AuthProvider.GitHub => "github",
            _ => throw new ArgumentOutOfRangeException(nameof(provider))
        };

        // Redirect to the backend OAuth endpoint
        var returnUrl = _navigation.ToAbsoluteUri("/").AbsoluteUri.TrimEnd('/');
        var oauthUrl = $"/api/auth/{providerName}/login?returnUrl={Uri.EscapeDataString(returnUrl)}";
        _navigation.NavigateTo(oauthUrl, forceLoad: true);

        return Task.FromResult(AuthResult.Ok(_currentState));
    }

    public Task<AuthResult> LogoutAsync()
    {
        _currentState = AuthState.Unauthenticated;
        AuthStateChanged?.Invoke(_currentState);

        // Redirect to logout endpoint
        _navigation.NavigateTo("/api/auth/logout", forceLoad: true);
        return Task.FromResult(AuthResult.Ok(_currentState));
    }

    /// <summary>
    /// Updates the current auth state (called after OAuth callback).
    /// </summary>
    public void UpdateAuthState(AuthState state)
    {
        _currentState = state;
        AuthStateChanged?.Invoke(state);
    }

    public void Dispose()
    {
        AuthStateChanged = null;
    }
}
