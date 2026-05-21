using Mnemi.Application.Models;
using Mnemi.Application.Ports;

namespace Mnemi.Application.Features.PinnedDecks;

/// <summary>
/// Handler for retrieving pinned decks.
/// </summary>
public class GetPinnedDecksHandler : IGetPinnedDecks
{
    /// <summary>
    /// Gets the list of pinned decks.
    /// </summary>
    /// <returns>A read-only list of pinned deck items.</returns>
    public Task<IReadOnlyList<PinnedDeckItem>> GetPinnedDecksAsync()
    {
        // TODO: Replace with actual data source (database, user preferences, etc.)
        var sampleDecks = new[]
        {
            new PinnedDeckItem("1", "Spanish Verbs", "/review/deck1"),
            new PinnedDeckItem("2", "World Capitals", "/review/deck2"),
            new PinnedDeckItem("3", "Periodic Table", "/review/deck3"),
        };

        return Task.FromResult<IReadOnlyList<PinnedDeckItem>>(sampleDecks);
    }
}
