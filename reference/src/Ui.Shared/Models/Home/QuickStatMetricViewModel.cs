namespace Mnemi.Ui.Shared.Models.Home;

public sealed record QuickStatMetricViewModel(
    string MetricId,
    string Label,
    string ValueText,
    string? TrendText,
    MetricTrendDirection TrendDirection,
    SectionDataState State);
