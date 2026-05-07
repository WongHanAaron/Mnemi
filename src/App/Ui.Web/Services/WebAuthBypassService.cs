using Ui.Shared.Services;

namespace Ui.Web.Services;

/// <summary>
/// Server-side auth bypass service. Checks for the E2E_TEST_AUTH_BYPASS flag
/// via ASP.NET Core configuration (env vars, appsettings.json, or CLI args).
/// </summary>
public class WebAuthBypassService : IAuthBypassService
{
    private readonly bool _bypassEnabled;

    public WebAuthBypassService(IConfiguration configuration)
    {
        // Supports:
        //   env var:  E2E_TEST_AUTH_BYPASS=1
        //   env var:  Auth__BypassForE2E=true   (ASP.NET Core convention)
        //   appsettings.json:  { "Auth": { "BypassForE2E": true } }
        //   CLI:  --Auth:BypassForE2E=true
        var raw = configuration["E2E_TEST_AUTH_BYPASS"]
               ?? configuration["Auth:BypassForE2E"];

        _bypassEnabled = !string.IsNullOrEmpty(raw)
            && (raw == "1" || raw.Equals("true", StringComparison.OrdinalIgnoreCase));
    }

    public bool IsAuthBypassed() => _bypassEnabled;
}
