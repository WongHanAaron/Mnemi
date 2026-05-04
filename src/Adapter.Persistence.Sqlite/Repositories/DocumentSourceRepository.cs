using Microsoft.EntityFrameworkCore;
using Mnemi.Domain.Entities;
using Mnemi.Application.Ports;

namespace Mnemi.Adapter.Persistence.Sqlite;

/// <summary>
/// SQLite implementation of IDocumentSourceRepository using Entity Framework Core.
/// </summary>
public class DocumentSourceRepository : IDocumentSourceRepository
{
    private readonly MnemiDbContext _context;

    public DocumentSourceRepository(MnemiDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<DocumentSourceConfig?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.DocumentSourceConfigs
            .AsNoTracking()
            .FirstOrDefaultAsync(ds => ds.Id == id, ct);

        return entity == null ? null : MapToDomain(entity);
    }

    public async Task<IReadOnlyList<DocumentSourceConfig>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var entities = await _context.DocumentSourceConfigs
            .AsNoTracking()
            .Where(ds => ds.UserId == userId)
            .OrderBy(ds => ds.DisplayName)
            .ToListAsync(ct);

        return entities.Select(MapToDomain).ToList();
    }

    public async Task<IReadOnlyList<DocumentSourceConfig>> GetByAuthConnectionIdAsync(Guid authConnectionId, CancellationToken ct = default)
    {
        var entities = await _context.DocumentSourceConfigs
            .AsNoTracking()
            .Where(ds => ds.AuthConnectionId == authConnectionId)
            .OrderBy(ds => ds.DisplayName)
            .ToListAsync(ct);

        return entities.Select(MapToDomain).ToList();
    }

    public async Task<bool> ExistsAsync(Guid userId, DocumentSourceProvider provider, string providerConfigKey, CancellationToken ct = default)
    {
        // Check if any document source exists for this user with the same provider
        // The providerConfigKey check would require parsing JSON, which is inefficient in SQL
        // For now, we do a basic check on user + provider
        return await _context.DocumentSourceConfigs
            .AsNoTracking()
            .AnyAsync(ds => ds.UserId == userId && ds.Provider == provider.ToString(), ct);
    }

    public async Task<DocumentSourceConfig> CreateAsync(DocumentSourceConfig source, CancellationToken ct = default)
    {
        var entity = new DocumentSourceConfigEntity
        {
            Id = source.Id,
            UserId = source.UserId,
            AuthConnectionId = source.AuthConnectionId,
            Provider = source.Provider.ToString(),
            DisplayName = source.DisplayName,
            ProviderConfigJson = source.ProviderConfigJson,
            IsAccessible = source.IsAccessible,
            LastErrorMessage = source.LastErrorMessage,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };

        _context.DocumentSourceConfigs.Add(entity);
        await _context.SaveChangesAsync(ct);
        return source;
    }

    public async Task UpdateAsync(DocumentSourceConfig source, CancellationToken ct = default)
    {
        var entity = await _context.DocumentSourceConfigs.FindAsync(new object[] { source.Id }, ct);

        if (entity == null)
        {
            throw new InvalidOperationException($"DocumentSourceConfig with ID {source.Id} not found");
        }

        entity.DisplayName = source.DisplayName;
        entity.ProviderConfigJson = source.ProviderConfigJson;
        entity.IsAccessible = source.IsAccessible;
        entity.LastErrorMessage = source.LastErrorMessage;
        entity.UpdatedAt = source.UpdatedAt;

        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.DocumentSourceConfigs.FindAsync(new object[] { id }, ct);
        if (entity != null)
        {
            _context.DocumentSourceConfigs.Remove(entity);
            await _context.SaveChangesAsync(ct);
        }
    }

    private static DocumentSourceConfig MapToDomain(DocumentSourceConfigEntity entity)
    {
        if (!Enum.TryParse<DocumentSourceProvider>(entity.Provider, out var provider))
        {
            throw new InvalidOperationException($"Unknown provider: {entity.Provider}");
        }

        return DocumentSourceConfig.Load(
            entity.Id,
            entity.UserId,
            entity.AuthConnectionId,
            provider,
            entity.DisplayName,
            entity.ProviderConfigJson,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.IsAccessible,
            entity.LastErrorMessage);
    }
}
