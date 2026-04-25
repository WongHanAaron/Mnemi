using System;
using System.Collections.Generic;

using Mnemi.Domain.Enums;

namespace Mnemi.Domain.Entities;

public class Deck
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DeckStatus Status { get; set; } = DeckStatus.Active;
    public bool IsPinned { get; set; } = false;
    private readonly List<Card> _cards = new();
    public IReadOnlyCollection<Card> Cards => _cards.AsReadOnly();
}
