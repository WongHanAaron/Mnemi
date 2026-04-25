using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mnemi.Application.Services;
using Mnemi.Domain.Entities;
using Mnemi.Domain.Enums;

namespace Mnemi.Application.Services.Implementations;

public class MockUserService : IUserService
{
    private readonly User _currentUser = new() { UserName = "Aaron", Email = "aaron@example.com" };

    public Task<User> GetCurrentUserAsync() => Task.FromResult(_currentUser);

    public Task UpdateUserAsync(User user)
    {
        _currentUser.UserName = user.UserName;
        _currentUser.Email = user.Email;
        return Task.CompletedTask;
    }
}

public class MockDeckService : IDeckService
{
    private readonly List<Deck> _decks = new()
    {
        new Deck { Title = "Biology 101", Description = "Intro to biology concepts", IsPinned = true },
        new Deck { Title = "Spanish Verbs", Description = "Common AR/ER/IR verbs", IsPinned = true },
        new Deck { Title = "World Capitals", Description = "Guess the capital city", IsPinned = false },
        new Deck { Title = "Chemistry Basics", Description = "Periodic table and more", IsPinned = false }
    };

    public Task<IReadOnlyCollection<Deck>> GetAllAsync() => Task.FromResult<IReadOnlyCollection<Deck>>(_decks.AsReadOnly());

    public Task<IReadOnlyCollection<Deck>> GetPinnedAsync() => Task.FromResult<IReadOnlyCollection<Deck>>(_decks.Where(d => d.IsPinned).ToList().AsReadOnly());

    public Task<Deck?> GetByIdAsync(Guid id) => Task.FromResult(_decks.FirstOrDefault(d => d.Id == id));

    public Task TogglePinAsync(Guid deckId)
    {
        var deck = _decks.FirstOrDefault(d => d.Id == deckId);
        if (deck != null) deck.IsPinned = !deck.IsPinned;
        return Task.CompletedTask;
    }
}

public class MockStatisticsService : IStatisticsService
{
    public Task<QuickStats> GetQuickStatsAsync(Guid userId)
    {
        return Task.FromResult(new QuickStats(42, 5, "Keep up the great work!"));
    }
}
