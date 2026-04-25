using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mnemi.Domain.Entities;

namespace Mnemi.Application.Services;

public interface IDeckService
{
    Task<IReadOnlyCollection<Deck>> GetAllAsync();
    Task<IReadOnlyCollection<Deck>> GetPinnedAsync();
    Task<Deck?> GetByIdAsync(Guid id);
    Task TogglePinAsync(Guid deckId);
}
