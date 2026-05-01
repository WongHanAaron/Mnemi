using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Mnemi.Ui.E2E.Playwright;

public abstract class PlaywrightTestBase : PageTest
{
    protected string BaseUrl => Environment.GetEnvironmentVariable("PLAYWRIGHT_BASE_URL")
        ?? "http://localhost:60458";

    protected Task GoToHomeAsync() => Page.GotoAsync($"{BaseUrl}/home");

    protected static string BuildDeckCardId(string rawDeckId)
    {
        if (string.IsNullOrWhiteSpace(rawDeckId))
        {
            return "unknown";
        }

        var filtered = rawDeckId.Where(char.IsLetterOrDigit).ToArray();
        return filtered.Length == 0 ? "unknown" : new string(filtered).ToLowerInvariant();
    }

    protected Task AssertVisibleAsync(ILocator locator) => Assertions.Expect(locator).ToBeVisibleAsync();
}
