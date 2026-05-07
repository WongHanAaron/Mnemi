using Mnemi.Application.Home;

namespace Mnemi.Ui.Maui.Services;

public sealed class HomeDashboardService : IHomeDashboardService
{
    public Task<HomeDashboardData> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var emptyDecks = Array.Empty<StudyDeckSummaryData>();
        var emptyStats = Array.Empty<QuickStatMetricData>();

        return Task.FromResult(new HomeDashboardData(
            Profile: new LearnerProfileSummaryData("Maui Learner", "Learner", "ML"),
            QuickStats: emptyStats,
            RecentDecks: emptyDecks,
            PinnedDecks: emptyDecks,
            PrimaryStudyAction: new HomePrimaryActionData("study-now", "Study now", "/study"),
            Mode: LayoutMode.DesktopHorizontal,
            IsAdaptiveFallbackActive: true,
            QuickStatsState: SectionDataState.Empty,
            RecentDecksState: SectionDataState.Empty,
            PinnedDecksState: SectionDataState.Empty,
            LastUpdatedUtc: DateTimeOffset.UtcNow));
    }
}
