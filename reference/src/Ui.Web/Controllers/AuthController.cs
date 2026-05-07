using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Mnemi.Application.Ports;
using Mnemi.Domain.Entities;
using Mnemi.Ui.Shared.Services;

namespace Mnemi.Ui.Web.Controllers;

/// <summary>
/// Handles OAuth authentication flows for Google and GitHub.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthConnectionRepository _authConnectionRepository;
    private readonly ITokenEncryptionService _tokenEncryptionService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUserRepository userRepository,
        IAuthConnectionRepository authConnectionRepository,
        ITokenEncryptionService tokenEncryptionService,
        ILogger<AuthController> logger)
    {
        _userRepository = userRepository;
        _authConnectionRepository = authConnectionRepository;
        _tokenEncryptionService = tokenEncryptionService;
        _logger = logger;
    }

    /// <summary>
    /// Initiates the OAuth login flow by redirecting to the provider.
    /// GET /api/auth/{provider}/login?returnUrl=...
    /// </summary>
    [HttpGet("{provider}/login")]
    public IActionResult Login(string provider, [FromQuery] string? returnUrl)
    {
        if (!IsValidProvider(provider))
        {
            return BadRequest(new { error = $"Unsupported provider: {provider}" });
        }

        var redirectUri = Url.Action(nameof(Callback), "Auth", new { provider }, Request.Scheme);
        var properties = new AuthenticationProperties
        {
            RedirectUri = redirectUri,
            Items =
            {
                { "returnUrl", returnUrl ?? "/" }
            }
        };

        return Challenge(properties, GetAuthenticationScheme(provider));
    }

    /// <summary>
    /// Handles the OAuth callback from the provider.
    /// GET /api/auth/{provider}/callback
    /// </summary>
    [HttpGet("{provider}/callback")]
    public async Task<IActionResult> Callback(string provider)
    {
        if (!IsValidProvider(provider))
        {
            return BadRequest(new { error = $"Unsupported provider: {provider}" });
        }

        try
        {
            // Authenticate the external provider result
            var authenticateResult = await HttpContext.AuthenticateAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded)
            {
                _logger.LogWarning("External auth failed for provider {Provider}", provider);
                return RedirectToLoginWithError("Authentication failed. Please try again.");
            }

            // Extract claims from the external provider
            var externalPrincipal = authenticateResult.Principal;
            var providerUserId = externalPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = externalPrincipal.FindFirstValue(ClaimTypes.Email);
            var displayName = externalPrincipal.FindFirstValue(ClaimTypes.Name);
            var accessToken = authenticateResult.Properties?.GetTokenValue("access_token");
            var refreshToken = authenticateResult.Properties?.GetTokenValue("refresh_token");
            var expiresAtStr = authenticateResult.Properties?.GetTokenValue("expires_at");
            var scopes = authenticateResult.Properties?.Items
                .Where(kv => kv.Key == ".Token.scope" || kv.Key.EndsWith(".scope"))
                .Select(kv => kv.Value)
                .FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("No email claim received from provider {Provider}", provider);
                return RedirectToLoginWithError("Could not retrieve email from provider. Please ensure you grant email access.");
            }

            var userEmail = email; // Non-null after the check above

            var oauthProvider = MapProvider(provider);
            DateTime? expiresAt = null;
            if (DateTime.TryParse(expiresAtStr, out var parsedExpiry))
            {
                expiresAt = parsedExpiry.ToUniversalTime();
            }

            // Encrypt tokens for storage
            var encryptedAccessToken = _tokenEncryptionService.Encrypt(accessToken ?? "");
            var encryptedRefreshToken = refreshToken != null
                ? _tokenEncryptionService.Encrypt(refreshToken)
                : null;

            // Find or create the user
            User user = null!;
            var existingConnection = await _authConnectionRepository
                .GetByProviderUserIdAsync(oauthProvider, providerUserId ?? "");

            if (existingConnection != null)
            {
                // Returning user — load their account
                user = (await _userRepository.GetByIdAsync(existingConnection.UserId))
                       ?? throw new InvalidOperationException("Auth connection exists but user not found");

                // Update tokens in the existing connection
                existingConnection.UpdateTokens(
                    encryptedAccessToken,
                    encryptedRefreshToken,
                    expiresAt,
                    scopes);
                await _authConnectionRepository.UpdateAsync(existingConnection);
            }
            else
            {
                // Check if user exists by email (could have signed up with a different provider)
                user = (await _userRepository.GetByEmailAsync(userEmail))!;
                if (user == null)
                {
                    // New user — create account
                    user = Mnemi.Domain.Entities.User.Create(userEmail, displayName ?? userEmail);
                    await _userRepository.CreateAsync(user);
                }

                // Create auth connection
                var connection = Mnemi.Domain.Entities.AuthConnection.Create(
                    user.Id,
                    oauthProvider,
                    providerUserId ?? "",
                    encryptedAccessToken,
                    encryptedRefreshToken,
                    expiresAt,
                    scopes);
                await _authConnectionRepository.CreateAsync(connection);
            }

            // Sign in with cookie authentication
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.DisplayName),
                new("provider", provider),
                new("avatar_url", externalPrincipal.FindFirstValue("urn:google:picture")
                    ?? externalPrincipal.FindFirstValue("avatar_url")
                    ?? "")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Clear the external auth and sign in with cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14)
                });

            _logger.LogInformation("User {UserId} signed in via {Provider}", user.Id, provider);

            // Find the return URL
            var returnUrl = authenticateResult.Properties?.Items
                .Where(kv => kv.Key == "returnUrl")
                .Select(kv => kv.Value)
                .FirstOrDefault() ?? "/";

            return LocalRedirect(returnUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during OAuth callback for provider {Provider}", provider);
            return RedirectToLoginWithError("An unexpected error occurred. Please try again.");
        }
    }

    /// <summary>
    /// Signs the user out.
    /// GET /api/auth/logout
    /// </summary>
    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return LocalRedirect("/login");
    }

    /// <summary>
    /// Returns the current authentication state as JSON.
    /// GET /api/auth/me
    /// </summary>
    [HttpGet("me")]
    public IActionResult Me()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return Ok(new AuthState { IsAuthenticated = false });
        }

        return Ok(new AuthState
        {
            IsAuthenticated = true,
            Email = User.FindFirstValue(ClaimTypes.Email),
            DisplayName = User.FindFirstValue(ClaimTypes.Name),
            AvatarUrl = User.FindFirstValue("avatar_url"),
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            Provider = User.FindFirstValue("provider")
        });
    }

    private IActionResult RedirectToLoginWithError(string errorMessage)
    {
        var encodedError = Uri.EscapeDataString(errorMessage);
        return LocalRedirect($"/login?error={encodedError}");
    }

    private static bool IsValidProvider(string provider) =>
        provider.Equals("google", StringComparison.OrdinalIgnoreCase) ||
        provider.Equals("github", StringComparison.OrdinalIgnoreCase);

    private static string GetAuthenticationScheme(string provider) =>
        provider.Equals("google", StringComparison.OrdinalIgnoreCase)
            ? GoogleDefaults.AuthenticationScheme
            : "GitHub";

    private static OAuthProvider MapProvider(string provider) =>
        provider.Equals("google", StringComparison.OrdinalIgnoreCase)
            ? OAuthProvider.Google
            : OAuthProvider.GitHub;
}
