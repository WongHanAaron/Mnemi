using NUnit.Framework;

namespace Mnemi.Ui.E2E.Playwright;

public sealed class HomeViewSelectorsTests : PlaywrightTestBase
{
    [Test]
    public async Task HomeSelectorContract_RemainsStable()
    {
        await GoToHomeAsync();

        await AssertVisibleAsync(Page.GetByTestId("home-shell"));
        await AssertVisibleAsync(Page.GetByTestId("home-primary-study-action"));
        await AssertVisibleAsync(Page.GetByTestId("home-quick-stats"));
        await AssertVisibleAsync(Page.GetByTestId("home-recent-decks"));
        await AssertVisibleAsync(Page.GetByTestId("home-pinned-decks"));

        await AssertVisibleAsync(Page.Locator("[data-testid^='deck-card-']").First);
        await AssertVisibleAsync(Page.Locator("[data-testid^='deck-card-action-']").First);
    }
}
