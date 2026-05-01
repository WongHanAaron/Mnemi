using NUnit.Framework;

namespace Mnemi.Ui.E2E.Playwright;

public sealed class HomeViewLayoutTests : PlaywrightTestBase
{
    [Test]
    public async Task HomeLayout_ExposesRequiredShellSelectors_OnDesktop()
    {
        await Page.SetViewportSizeAsync(1440, 900);
        await GoToHomeAsync();

        await AssertVisibleAsync(Page.GetByTestId("home-shell"));
        await AssertVisibleAsync(Page.GetByTestId("home-sidenav"));
        await AssertVisibleAsync(Page.GetByTestId("home-welcome"));
        await AssertVisibleAsync(Page.GetByTestId("home-quick-stats"));
        await AssertVisibleAsync(Page.GetByTestId("home-recent-decks"));
        await AssertVisibleAsync(Page.GetByTestId("home-pinned-decks"));
    }
}
