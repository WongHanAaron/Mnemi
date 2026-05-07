namespace Ui.Shared.Services;

/// <summary>
/// Provides a feature-flag check for auth bypass, used in E2E tests.
/// Injected via DI — mockable in tests, configurable from environment or settings.
/// </summary>
public interface IAuthBypassService
{
    /// <summary>
    /// Returns true when authentication should be bypassed,
    /// allowing the UI to render without a logged-in user.
    /// </summary>
    bool IsAuthBypassed();
}
