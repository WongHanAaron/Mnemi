using Ui.Shared.Services;

namespace Ui.Services;

/// <summary>
/// MAUI auth bypass service. Reads E2E_TEST_AUTH_BYPASS from environment.
/// The MAUI MauiAuthService already returns authenticated, so bypass is rarely needed,
/// but this provides consistency with the web app pattern.
/// </summary>
public class MauiAuthBypassService : IAuthBypassService
{
    public bool IsAuthBypassed()
    {
        var raw = Environment.GetEnvironmentVariable("E2E_TEST_AUTH_BYPASS");
        return !string.IsNullOrEmpty(raw);
    }
}
