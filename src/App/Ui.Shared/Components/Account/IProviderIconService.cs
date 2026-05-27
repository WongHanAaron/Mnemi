using Microsoft.AspNetCore.Components;

namespace Ui.Shared.Components.Account;

/// <summary>
/// Service for resolving provider icons. Allows platform-specific
/// overrides (e.g., SVG on Web, native icons on MAUI).
/// </summary>
public interface IProviderIconService
{
    /// <summary>Returns SVG markup for the given provider's icon.</summary>
    MarkupString GetProviderIcon(string providerName);

    /// <summary>Returns SVG icon + label markup for a document source provider.</summary>
    MarkupString GetSourceProviderLabel(string providerName);
}

/// <summary>
/// Default implementation using SVG icons consistent with the project's
/// existing SidebarIcon pattern (24x24, stroke-based, currentColor).
/// </summary>
public sealed class DefaultProviderIconService : IProviderIconService
{
    // 24x24 Google "G" logo — simplified single-letter style
    private const string GoogleSvg =
        """<svg class="provider-icon" viewBox="0 0 24 24" width="1.25rem" height="1.25rem" xmlns="http://www.w3.org/2000/svg" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><circle cx="12" cy="12" r="10"/><path d="M8 12h8M12 8v8"/></svg>""";

    // 24x24 GitHub mark — simplified octocat-style
    private const string GitHubSvg =
        """<svg class="provider-icon" viewBox="0 0 24 24" width="1.25rem" height="1.25rem" xmlns="http://www.w3.org/2000/svg" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M15 22v-4a4.8 4.8 0 0 0-1-3.5c3 0 6-2 6-5.5.08-1.25-.27-2.48-1-3.5.28-1.15.28-2.35 0-3.5 0 0-1 0-3 1.5-2.64-.5-5.36-.5-8 0C6 2 5 2 5 2c-.3 1.15-.3 2.35 0 3.5A5.403 5.403 0 0 0 4 9c0 3.5 3 5.5 6 5.5-.39.49-.68 1.05-.85 1.65-.17.6-.22 1.23-.15 1.85v4"/><path d="M9 18c-4.51 2-5-2-7-2"/></svg>""";

    // 24x24 drive / folder icon for Google Drive
    private const string GoogleDriveSvg =
        """<svg class="provider-icon" viewBox="0 0 24 24" width="1.25rem" height="1.25rem" xmlns="http://www.w3.org/2000/svg" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M22 19a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h5l2 3h9a2 2 0 0 1 2 2z"/></svg>""";

    // Generic link icon for unknown providers
    private const string GenericSvg =
        """<svg class="provider-icon" viewBox="0 0 24 24" width="1.25rem" height="1.25rem" xmlns="http://www.w3.org/2000/svg" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M10 13a5 5 0 0 0 7.54.54l3-3a5 5 0 0 0-7.07-7.07l-1.72 1.71"/><path d="M14 11a5 5 0 0 0-7.54-.54l-3 3a5 5 0 0 0 7.07 7.07l1.71-1.71"/></svg>""";

    public MarkupString GetProviderIcon(string providerName)
    {
        var svg = providerName switch
        {
            "Google" => GoogleSvg,
            "GitHub" => GitHubSvg,
            _ => GenericSvg
        };
        return new MarkupString(svg);
    }

    public MarkupString GetSourceProviderLabel(string providerName)
    {
        var svg = providerName switch
        {
            "GoogleDrive" => GoogleDriveSvg,
            "GitHub" => GitHubSvg,
            _ => GenericSvg
        };
        var label = providerName switch
        {
            "GoogleDrive" => "Google Drive",
            _ => providerName
        };
        return new MarkupString($"""<span class="source-row__provider-badge-inner">{svg}<span>{label}</span></span>""");
    }
}

