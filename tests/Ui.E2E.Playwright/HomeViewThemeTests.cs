using NUnit.Framework;

namespace Mnemi.Ui.E2E.Playwright;

public sealed class HomeViewThemeTests : PlaywrightTestBase
{
    [Test]
    public async Task HomeDeckCards_KeepThemeStructureClasses()
    {
        await Page.SetViewportSizeAsync(1440, 900);
        await GoToHomeAsync();

        var firstDeckCard = Page.Locator(".home-deck-card").First;
        await AssertVisibleAsync(firstDeckCard);
        await AssertVisibleAsync(firstDeckCard.Locator(".home-deck-card__cover"));
        await AssertVisibleAsync(firstDeckCard.Locator(".home-deck-card__meta"));
    }
}
