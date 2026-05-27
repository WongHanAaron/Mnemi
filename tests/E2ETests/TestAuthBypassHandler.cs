using Microsoft.Playwright;

namespace E2ETests;

/// <summary>
/// Helper to ensure auth bypass is active before each test.
/// The server checks E2E_TEST_AUTH_BYPASS env var; this navigates to
/// the app first to confirm the bypass is working.
/// </summary>
public static class TestAuthBypassHandler
{
    public static async Task SetBypassAsync(IPage page)
    {
        // Navigate to trigger the server-side auth bypass
        await page.GotoAsync($"{AppTestFixture.BaseUrl}/");
        // Wait for the sidebar user section to confirm auth passed
        await page.WaitForSelectorAsync("[data-testid=\"sidebar-user-section\"]",
            new PageWaitForSelectorOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15000
            });
    }
}
