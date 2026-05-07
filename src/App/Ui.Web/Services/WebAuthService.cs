using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Ui.Shared.Services;

namespace Ui.Web.Services;

/// <summary>
/// Web-specific implementation of IAuthService for Blazor Server.
/// Manages auth state via cookie authentication and server-side OAuth redirects.
/// </summary>
public class WebAuthService : IAuthService, IDisposable
{
    private readonly NavigationManager _navigation;
    private readonly IHttpClientFactory _httpClientFactory;
    private AuthState _currentState = AuthState.Unauthenticated;
    private bool _initialized;

    public WebAuthService(NavigationManager navigation, IHttpClientFactory httpClientFactory)
    {
        _navigation = navigation;
        _httpClientFactory = httpClientFactory;
    }

    public event Action<AuthState>? AuthStateChanged;

    public async Task<AuthState> GetCurrentAuthStateAsync()
    {
        if (!_initialized)
        {
            await RefreshAuthStateAsync();
            _initialized = true;
        }

        return _currentState;
    }

    public Task<AuthResult> LoginAsync(AuthProvider provider)
    {
        var providerName = provider switch
        {
            AuthProvider.Google => "google",
            AuthProvider.GitHub => "github",
            _ => throw new ArgumentOutOfRangeException(nameof(provider))
        };

        // Construct the return URL from the current page
        var uri = new Uri(_navigation.Uri);
        var returnUrl = uri.GetLeftPart(UriPartial.Authority);

        // Redirect to the backend OAuth challenge endpoint
        var oauthUrl = $"/api/auth/{providerName}/login?returnUrl={Uri.EscapeDataString(returnUrl)}";
        _navigation.NavigateTo(oauthUrl, forceLoad: true);

        return Task.FromResult(AuthResult.Ok(_currentState));
    }

    public Task<AuthResult> LogoutAsync()
    {
        _currentState = AuthState.Unauthenticated;
        AuthStateChanged?.Invoke(_currentState);

        // Redirect to logout endpoint which clears the cookie
        _navigation.NavigateTo("/api/auth/logout", forceLoad: true);
        return Task.FromResult(AuthResult.Ok(_currentState));
    }

    /// <summary>
    /// Refreshes auth state from the server /api/auth/me endpoint.
    /// </summary>
    public async Task RefreshAuthStateAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var state = await client.GetFromJsonAsync<AuthState>("/api/auth/me");
            if (state != null)
            {
                _currentState = state;
                AuthStateChanged?.Invoke(state);
            }
        }
        catch
        {
            _currentState = AuthState.Unauthenticated;
        }
    }

    /// <summary>
    /// Updates the current auth state directly (used for internal state management).
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
