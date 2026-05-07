using Microsoft.EntityFrameworkCore;
using Mnemi.Domain.Entities;
using Mnemi.Application.Ports;

namespace Mnemi.Adapter.Persistence.Sqlite;

/// <summary>
/// SQLite implementation of IUserRepository using Entity Framework Core.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly MnemiDbContext _context;

    public UserRepository(MnemiDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct);

        return entity == null ? null : MapToDomain(entity);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var entity = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, ct);

        return entity == null ? null : MapToDomain(entity);
    }

    public async Task<User?> GetByAuthConnectionAsync(OAuthProvider provider, string providerUserId, CancellationToken ct = default)
    {
        var entity = await _context.Users
            .AsNoTracking()
            .Join(
                _context.AuthConnections.Where(ac =>
                    ac.Provider == provider.ToString() &&
                    ac.ProviderUserId == providerUserId),
                u => u.Id,
                ac => ac.UserId,
                (u, ac) => u)
            .FirstOrDefaultAsync(ct);

        return entity == null ? null : MapToDomain(entity);
    }

    public async Task<User> CreateAsync(User user, CancellationToken ct = default)
    {
        var entity = new UserEntity
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            CreatedAt = user.CreatedAt
        };

        _context.Users.Add(entity);
        await _context.SaveChangesAsync(ct);
        return user;
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        var entity = await _context.Users.FindAsync(new object[] { user.Id }, ct);

        if (entity == null)
        {
            throw new InvalidOperationException($"User with ID {user.Id} not found");
        }

        entity.DisplayName = user.DisplayName;

        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.Users.FindAsync(new object[] { id }, ct);
        if (entity != null)
        {
            _context.Users.Remove(entity);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == email, ct);
    }

    private static User MapToDomain(UserEntity entity)
    {
        return User.Load(entity.Id, entity.Email, entity.DisplayName, entity.CreatedAt);
    }
}
