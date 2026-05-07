namespace Mnemi.Domain.ValueObjects;

/// <summary>
/// Configuration specific to GitHub repository document sources.
/// </summary>
public class GitHubProviderConfig
{
    /// <summary>
    /// Repository owner (user or organization name).
    /// </summary>
    public string RepoOwner { get; set; } = string.Empty;

    /// <summary>
    /// Repository name.
    /// </summary>
    public string RepoName { get; set; } = string.Empty;

    /// <summary>
    /// Optional root path within the repository.
    /// Defaults to "/" for repository root.
    /// Example: "/notes/flashcards"
    /// </summary>
    public string RootPath { get; set; } = "/";

    /// <summary>
    /// Optional branch name.
    /// Defaults to "main" or "master".
    /// </summary>
    public string? Branch { get; set; }
}
