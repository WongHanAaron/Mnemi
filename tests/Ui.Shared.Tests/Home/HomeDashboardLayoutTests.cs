using Bunit;
using Mnemi.Ui.Shared.Components.Home;
using Mnemi.Ui.Shared.Models.Home;
using Xunit;

namespace Mnemi.Ui.Shared.Tests.Home;

public sealed class HomeDashboardLayoutTests : TestContext
{
    [Fact]
    public void HomeDashboard_RendersRequiredDesktopSections()
    {
        var model = BuildPopulatedModel();

        var cut = RenderComponent<HomeDashboard>(parameters => parameters
            .Add(component => component.Model, model));

        Assert.NotNull(cut.Find("[data-testid='home-shell']"));
        Assert.NotNull(cut.Find("[data-testid='home-sidenav']"));
        Assert.NotNull(cut.Find("[data-testid='home-welcome']"));
        Assert.NotNull(cut.Find("[data-testid='home-quick-stats']"));
        Assert.NotNull(cut.Find("[data-testid='home-recent-decks']"));
        Assert.NotNull(cut.Find("[data-testid='home-pinned-decks']"));
        Assert.NotNull(cut.Find("[data-testid='home-primary-study-action']"));
    }

    private static HomeDashboardViewModel BuildPopulatedModel() => new(
        Profile: new LearnerProfileSummaryViewModel("Avery", "Learner", "AV"),
        QuickStats:
        [
            new QuickStatMetricViewModel("due", "Due", "10", "+2", MetricTrendDirection.Up, SectionDataState.Populated)
        ],
        RecentDecks:
        [
            new StudyDeckSummaryViewModel(
                "deck-one",
                "Deck One",
                "Subtitle",
                40,
                "Learning",
                "D1",
                4,
                false,
                new DeckPrimaryAction("open", "Open", "deck-one"),
                SectionDataState.Populated)
        ],
        PinnedDecks:
        [
            new StudyDeckSummaryViewModel(
                "deck-two",
                "Deck Two",
                "Pinned",
                75,
                "Strong",
                "D2",
                1,
                true,
                new DeckPrimaryAction("open", "Open", "deck-two"),
                SectionDataState.Populated)
        ],
        PrimaryStudyAction: new HomePrimaryAction("study-now", "Study now", "/study"),
        Mode: LayoutMode.DesktopHorizontal,
        IsAdaptiveFallbackActive: false,
        QuickStatsState: SectionDataState.Populated,
        RecentDecksState: SectionDataState.Populated,
        PinnedDecksState: SectionDataState.Populated,
        LastUpdatedUtc: DateTimeOffset.UtcNow);
}
