namespace Mnemi.Application.Home;

public interface IHomeDashboardService
{
    Task<HomeDashboardData> GetDashboardAsync(CancellationToken cancellationToken = default);
}

public sealed record HomeDashboardData(
    LearnerProfileSummaryData Profile,
    IReadOnlyList<QuickStatMetricData> QuickStats,
    IReadOnlyList<StudyDeckSummaryData> RecentDecks,
    IReadOnlyList<StudyDeckSummaryData> PinnedDecks,
    HomePrimaryActionData PrimaryStudyAction,
    LayoutMode Mode,
    bool IsAdaptiveFallbackActive,
    SectionDataState QuickStatsState,
    SectionDataState RecentDecksState,
    SectionDataState PinnedDecksState,
    DateTimeOffset LastUpdatedUtc);

public sealed record LearnerProfileSummaryData(string? DisplayName, string GreetingFallback, string? AvatarInitials);

public sealed record QuickStatMetricData(
    string MetricId,
    string Label,
    string ValueText,
    string? TrendText,
    MetricTrendDirection TrendDirection,
    SectionDataState State);

public sealed record StudyDeckSummaryData(
    string DeckId,
    string Title,
    string? Subtitle,
    int? ProgressPercent,
    string? StatusLabel,
    string? ArtworkToken,
    int? DueCount,
    bool IsPinned,
    DeckPrimaryActionData PrimaryAction,
    SectionDataState State);

public sealed record HomePrimaryActionData(string ActionId, string Label, string TargetRoute);

public sealed record DeckPrimaryActionData(string ActionId, string Label, string PayloadDeckId);

public enum SectionDataState
{
    Loading,
    Populated,
    Empty,
    MissingData
}

public enum LayoutMode
{
    DesktopHorizontal,
    TabletAdaptive,
    PhoneStacked
}

public enum MetricTrendDirection
{
    Up,
    Flat,
    Down,
    Unknown
}
