namespace Mnemi.Ui.Shared.Models;

/// <summary>
/// Represents a navigation item in the sidebar, such as a quick link.
/// </summary>
public sealed record SidebarNavItem(
    string Id,
    string Label,
    string Href,
    string IconKind = ""
);
