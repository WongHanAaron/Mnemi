using Mnemi.Application.Home;

namespace Mnemi.Ui.Web.Services;

public sealed class HomeDashboardStubDataProvider
{
    public HomeDashboardData BuildDashboard()
    {
        var quickStats = new List<QuickStatMetricData>
        {
            new("retention", "Retention", "87%", "+4% this week", MetricTrendDirection.Up, SectionDataState.Populated),
            new("streak", "Current streak", "12 days", "Stay focused", MetricTrendDirection.Flat, SectionDataState.Populated),
            new("due", "Due today", "23 cards", "-3 from yesterday", MetricTrendDirection.Down, SectionDataState.Populated)
        };

        var recentDecks = new List<StudyDeckSummaryData>
        {
            new(
                DeckId: "spanish-basics",
                Title: "Spanish Basics",
                Subtitle: "Greetings and travel",
                ProgressPercent: 61,
                StatusLabel: "Review due",
                ArtworkToken: "SP",
                DueCount: 8,
                IsPinned: false,
                PrimaryAction: new DeckPrimaryActionData("resume", "Resume", "spanish-basics"),
                State: SectionDataState.Populated),
            new(
                DeckId: "biology-keyterms",
                Title: "Biology Key Terms",
                Subtitle: "Cell and DNA",
                ProgressPercent: 34,
                StatusLabel: "Learning",
                ArtworkToken: "BIO",
                DueCount: 15,
                IsPinned: false,
                PrimaryAction: new DeckPrimaryActionData("resume", "Resume", "biology-keyterms"),
                State: SectionDataState.Populated)
        };

        var pinnedDecks = new List<StudyDeckSummaryData>
        {
            new(
                DeckId: "csharp-patterns",
                Title: "C# Patterns",
                Subtitle: "SOLID and architecture",
                ProgressPercent: 78,
                StatusLabel: "Strong",
                ArtworkToken: "C#",
                DueCount: 4,
                IsPinned: true,
                PrimaryAction: new DeckPrimaryActionData("open", "Open", "csharp-patterns"),
                State: SectionDataState.Populated),
            new(
                DeckId: "history-dates",
                Title: "History Dates",
                Subtitle: "World events",
                ProgressPercent: null,
                StatusLabel: null,
                ArtworkToken: null,
                DueCount: null,
                IsPinned: true,
                PrimaryAction: new DeckPrimaryActionData("open", "Open", "history-dates"),
                State: SectionDataState.MissingData)
        };

        return new HomeDashboardData(
            Profile: new LearnerProfileSummaryData("Avery", "Learner", "AV"),
            QuickStats: quickStats,
            RecentDecks: recentDecks,
            PinnedDecks: pinnedDecks,
            PrimaryStudyAction: new HomePrimaryActionData("study-now", "Study now", "/study"),
            Mode: LayoutMode.DesktopHorizontal,
            IsAdaptiveFallbackActive: false,
            QuickStatsState: SectionDataState.Populated,
            RecentDecksState: SectionDataState.Populated,
            PinnedDecksState: SectionDataState.Populated,
            LastUpdatedUtc: DateTimeOffset.UtcNow);
    }
}
