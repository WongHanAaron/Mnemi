using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mnemi.Domain.Entities;

namespace Mnemi.Domain.Repositories;

public interface IDeckRepository
{
    Task<IReadOnlyCollection<Deck>> GetAllAsync();
    Task<Deck?> GetByIdAsync(Guid id);
    Task<IReadOnlyCollection<Deck>> GetPinnedAsync();
    Task<Deck> AddAsync(Deck deck);
    Task<Deck> UpdateAsync(Deck deck);
    Task DeleteAsync(Guid id);
}
