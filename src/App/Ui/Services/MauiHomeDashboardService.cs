using Mnemi.Application.Home;

namespace Ui.Services;

/// <summary>
/// MAUI stub IHomeDashboardService. Returns sample dashboard data.
/// </summary>
public class MauiHomeDashboardService : IHomeDashboardService
{
    public Task<HomeDashboardData> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var data = new HomeDashboardData(
            Profile: new LearnerProfileSummaryData("Student", "Learner", null),
            QuickStats: new[]
            {
                new QuickStatMetricData("retention", "Retention", "87%", "+4% this week", MetricTrendDirection.Up, SectionDataState.Populated),
                new QuickStatMetricData("streak", "Current streak", "12 days", "Stay focused", MetricTrendDirection.Flat, SectionDataState.Populated),
                new QuickStatMetricData("due", "Due today", "23 cards", "-3 from yesterday", MetricTrendDirection.Down, SectionDataState.Populated),
            },
            RecentDecks: Array.Empty<StudyDeckSummaryData>(),
            PinnedDecks: Array.Empty<StudyDeckSummaryData>(),
            PrimaryStudyAction: new HomePrimaryActionData("study-now", "Study now", "/study"),
            Mode: LayoutMode.DesktopHorizontal,
            IsAdaptiveFallbackActive: false,
            QuickStatsState: SectionDataState.Populated,
            RecentDecksState: SectionDataState.Empty,
            PinnedDecksState: SectionDataState.Empty,
            LastUpdatedUtc: DateTimeOffset.UtcNow);

        return Task.FromResult(data);
    }
}
