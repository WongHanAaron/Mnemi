namespace Mnemi.Domain.Entities;

/// <summary>
/// Identifies the external OAuth provider.
/// </summary>
public enum OAuthProvider
{
    /// <summary>Google OAuth provider for Google Drive access</summary>
    Google = 1,

    /// <summary>GitHub OAuth provider for repository access</summary>
    GitHub = 2
}
