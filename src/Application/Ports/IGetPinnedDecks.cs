using Mnemi.Application.Models;

namespace Mnemi.Application.Ports;

/// <summary>
/// Service interface for retrieving pinned flashcard decks.
/// </summary>
public interface IGetPinnedDecks
{
    /// <summary>
    /// Gets the list of pinned decks for the current user.
    /// </summary>
    /// <returns>A read-only list of pinned deck items.</returns>
    Task<IReadOnlyList<PinnedDeckItem>> GetPinnedDecksAsync();
}
