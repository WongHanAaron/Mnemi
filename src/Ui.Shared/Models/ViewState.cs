namespace Mnemi.Ui.Shared.Models;

/// <summary>
/// Represents the current layout viewing state based on viewport width.
/// Determines how the sidebar and content layout adapt to screen size.
/// </summary>
public enum ViewState
{
    /// <summary>Narrow width (≤767px) — stacked layout, sidebar hidden by default.</summary>
    Phone,

    /// <summary>Medium width (768–1023px) — compact sidebar, icons only.</summary>
    Tablet,

    /// <summary>Wide width (≥1024px) — full sidebar, all content visible.</summary>
    Desktop
}

/// <summary>
/// Centralized viewport breakpoints used by all layout components.
/// </summary>
public static class ViewBreakpoints
{
    /// <summary>Viewports ≤ 767px are considered Phone.</summary>
    public const int PhoneMax = 767;

    /// <summary>Viewports 768–1023px are considered Tablet.</summary>
    public const int TabletMax = 1023;

    /// <summary>Maps a viewport width to the corresponding ViewState.</summary>
    public static ViewState GetViewState(double width) =>
        width <= PhoneMax ? ViewState.Phone :
        width <= TabletMax ? ViewState.Tablet :
        ViewState.Desktop;
}
