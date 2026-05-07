namespace Mnemi.Ui.Shared.Models.Home;

public sealed record StudyDeckSummaryViewModel(
    string DeckId,
    string Title,
    string? Subtitle,
    int? ProgressPercent,
    string? StatusLabel,
    string? ArtworkToken,
    int? DueCount,
    bool IsPinned,
    DeckPrimaryAction PrimaryAction,
    SectionDataState State);
