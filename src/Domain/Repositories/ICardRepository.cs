using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mnemi.Domain.Entities;

namespace Mnemi.Domain.Repositories;

public interface ICardRepository
{
    Task<IReadOnlyCollection<Card>> GetByDeckIdAsync(Guid deckId);
    Task<Card?> GetByIdAsync(Guid id);
    Task<Card> AddAsync(Card card);
    Task<Card> UpdateAsync(Card card);
    Task DeleteAsync(Guid id);
}
