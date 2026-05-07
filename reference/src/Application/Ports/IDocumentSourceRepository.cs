using Mnemi.Domain.Entities;

namespace Mnemi.Application.Ports;

/// <summary>
/// Repository interface for DocumentSourceConfig operations.
/// </summary>
public interface IDocumentSourceRepository
{
    /// <summary>
    /// Gets a document source by its unique identifier.
    /// </summary>
    Task<DocumentSourceConfig?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets all document sources for a user.
    /// </summary>
    Task<IReadOnlyList<DocumentSourceConfig>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Gets all document sources for an auth connection.
    /// </summary>
    Task<IReadOnlyList<DocumentSourceConfig>> GetByAuthConnectionIdAsync(Guid authConnectionId, CancellationToken ct = default);

    /// <summary>
    /// Checks if a document source already exists for the user with the same provider configuration.
    /// </summary>
    Task<bool> ExistsAsync(Guid userId, DocumentSourceProvider provider, string providerConfigKey, CancellationToken ct = default);

    /// <summary>
    /// Creates a new document source.
    /// </summary>
    Task<DocumentSourceConfig> CreateAsync(DocumentSourceConfig source, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing document source.
    /// </summary>
    Task UpdateAsync(DocumentSourceConfig source, CancellationToken ct = default);

    /// <summary>
    /// Deletes a document source by its ID.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
