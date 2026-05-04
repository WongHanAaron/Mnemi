namespace Mnemi.Domain.Entities;

/// <summary>
/// Aggregate root representing a person using Mnemi.
/// </summary>
public class User : Entity
{
    /// <summary>
    /// User's email address (from OAuth provider).
    /// Used for account matching across providers.
    /// </summary>
    public string Email { get; private set; }

    /// <summary>
    /// Display name shown in the UI.
    /// Defaults to OAuth provider name but can be customized.
    /// </summary>
    public string DisplayName { get; private set; }

    /// <summary>
    /// UTC timestamp when the user account was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Navigation property for OAuth connections.
    /// A user can have multiple connections (Google + GitHub).
    /// </summary>
    public ICollection<AuthConnection> AuthConnections { get; private set; } = new List<AuthConnection>();

    /// <summary>
    /// Navigation property for document sources.
    /// A user can have multiple sources across providers.
    /// </summary>
    public ICollection<DocumentSourceConfig> DocumentSources { get; private set; } = new List<DocumentSourceConfig>();

    // EF Core requires a parameterless constructor
    private User() { }

    private User(Guid id, string email, string displayName, DateTime createdAt)
    {
        Id = id;
        Email = email;
        DisplayName = displayName;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Factory method to create a new user.
    /// </summary>
    public static User Create(string email, string displayName)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name is required", nameof(displayName));
        if (displayName.Length > 100)
            throw new ArgumentException("Display name must be 100 characters or less", nameof(displayName));

        return new User(Guid.NewGuid(), email.Trim(), displayName.Trim(), DateTime.UtcNow);
    }

    /// <summary>
    /// Loads an existing user from persistence. Used by repositories.
    /// </summary>
    public static User Load(Guid id, string email, string displayName, DateTime createdAt)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("ID is required", nameof(id));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name is required", nameof(displayName));

        return new User(id, email.Trim(), displayName.Trim(), createdAt);
    }

    /// <summary>
    /// Updates the user's display name.
    /// </summary>
    public void UpdateDisplayName(string newDisplayName)
    {
        if (string.IsNullOrWhiteSpace(newDisplayName))
            throw new ArgumentException("Display name is required", nameof(newDisplayName));
        if (newDisplayName.Length > 100)
            throw new ArgumentException("Display name must be 100 characters or less", nameof(newDisplayName));

        DisplayName = newDisplayName.Trim();
    }

    /// <summary>
    /// Determines if the specified auth connection can be removed.
    /// Users must maintain at least one auth connection.
    /// </summary>
    public bool CanRemoveConnection(AuthConnection connection)
    {
        return AuthConnections.Count > 1;
    }

    /// <summary>
    /// Adds an auth connection to this user.
    /// </summary>
    public void AddAuthConnection(AuthConnection connection)
    {
        if (connection == null)
            throw new ArgumentNullException(nameof(connection));

        AuthConnections.Add(connection);
    }

    /// <summary>
    /// Adds a document source to this user.
    /// </summary>
    public void AddDocumentSource(DocumentSourceConfig source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        DocumentSources.Add(source);
    }
}
