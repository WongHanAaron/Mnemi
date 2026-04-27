using Bunit;
using Mnemi.Ui.Shared.Components.Home;
using Mnemi.Ui.Shared.Models.Home;
using Xunit;

namespace Mnemi.Ui.Shared.Tests.Home;

public sealed class HomeDashboardStateTests : TestContext
{
    [Fact]
    public void HomeDashboard_RendersLoadingStateSelectors()
    {
        var cut = RenderComponent<HomeDashboard>(parameters => parameters
            .Add(component => component.Model, BuildModel(
                quickStatsState: SectionDataState.Loading,
                recentState: SectionDataState.Loading,
                pinnedState: SectionDataState.Loading)));

        Assert.NotNull(cut.Find("[data-testid='home-state-loading-quick-stats']"));
        Assert.NotNull(cut.Find("[data-testid='home-state-loading-recent-decks']"));
        Assert.NotNull(cut.Find("[data-testid='home-state-loading-pinned-decks']"));
    }

    [Fact]
    public void HomeDashboard_RendersEmptyStateSelectors()
    {
        var cut = RenderComponent<HomeDashboard>(parameters => parameters
            .Add(component => component.Model, BuildModel(
                quickStatsState: SectionDataState.Empty,
                recentState: SectionDataState.Empty,
                pinnedState: SectionDataState.Empty)));

        Assert.NotNull(cut.Find("[data-testid='home-state-empty-quick-stats']"));
        Assert.NotNull(cut.Find("[data-testid='home-state-empty-recent-decks']"));
        Assert.NotNull(cut.Find("[data-testid='home-state-empty-pinned-decks']"));
    }

    [Fact]
    public void HomeDashboard_RendersMissingStateSelectorsAndFallbackName()
    {
        var model = BuildModel(
            quickStatsState: SectionDataState.MissingData,
            recentState: SectionDataState.MissingData,
            pinnedState: SectionDataState.MissingData) with
        {
            Profile = new LearnerProfileSummaryViewModel(null, "Learner", null)
        };

        var cut = RenderComponent<HomeDashboard>(parameters => parameters
            .Add(component => component.Model, model));

        Assert.NotNull(cut.Find("[data-testid='home-state-missing-quick-stats']"));
        Assert.NotNull(cut.Find("[data-testid='home-state-missing-recent-decks']"));
        Assert.NotNull(cut.Find("[data-testid='home-state-missing-pinned-decks']"));
        Assert.Contains("Welcome back, Learner", cut.Markup);
    }

    private static HomeDashboardViewModel BuildModel(
        SectionDataState quickStatsState,
        SectionDataState recentState,
        SectionDataState pinnedState) => new(
        Profile: new LearnerProfileSummaryViewModel("Avery", "Learner", "AV"),
        QuickStats: Array.Empty<QuickStatMetricViewModel>(),
        RecentDecks: Array.Empty<StudyDeckSummaryViewModel>(),
        PinnedDecks: Array.Empty<StudyDeckSummaryViewModel>(),
        PrimaryStudyAction: new HomePrimaryAction("study-now", "Study now", "/study"),
        Mode: LayoutMode.DesktopHorizontal,
        IsAdaptiveFallbackActive: false,
        QuickStatsState: quickStatsState,
        RecentDecksState: recentState,
        PinnedDecksState: pinnedState,
        LastUpdatedUtc: DateTimeOffset.UtcNow);
}
