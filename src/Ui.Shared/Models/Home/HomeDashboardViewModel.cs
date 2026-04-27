namespace Mnemi.Ui.Shared.Models.Home;

public sealed record HomeDashboardViewModel(
    LearnerProfileSummaryViewModel Profile,
    IReadOnlyList<QuickStatMetricViewModel> QuickStats,
    IReadOnlyList<StudyDeckSummaryViewModel> RecentDecks,
    IReadOnlyList<StudyDeckSummaryViewModel> PinnedDecks,
    HomePrimaryAction PrimaryStudyAction,
    LayoutMode Mode,
    bool IsAdaptiveFallbackActive,
    SectionDataState QuickStatsState,
    SectionDataState RecentDecksState,
    SectionDataState PinnedDecksState,
    DateTimeOffset LastUpdatedUtc);
