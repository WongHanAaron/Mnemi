namespace Mnemi.Ui.Shared.Models;

/// <summary>
/// Represents a deck that the user has pinned for quick access in the sidebar.
/// </summary>
public sealed record PinnedDeckItem(
    string Id,
    string Name,
    string Href
);
