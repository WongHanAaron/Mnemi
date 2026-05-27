using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace E2ETests;

/// <summary>
/// E2E tests for the Account Settings page.
///
/// The app is started automatically by AppTestFixture (SetUpFixture).
/// Auth bypass is handled via E2E_TEST_AUTH_BYPASS=true.
/// </summary>
[TestFixture]
public class AccountSettingsPageTests : PageTest
{
    [SetUp]
    public async Task SetUp()
    {
        await TestAuthBypassHandler.SetBypassAsync(Page);
    }

    private static string Url(string path) => $"{AppTestFixture.BaseUrl}{path}";

    // ─────────────────────────────────────────────────
    // V1.1 — Navigation via Sidebar Avatar
    // ─────────────────────────────────────────────────

    [Test]
    public async Task NavigateToAccount_FromSidebarAvatar_ShowsAccountPage()
    {
        await Page.WaitForSelectorAsync("[data-testid=\"sidebar-user-section\"]");

        await Page.Locator("[data-testid=\"sidebar-user-section\"][role=\"button\"]").ClickAsync();

        await Expect(Page).ToHaveURLAsync(Url("/account"));
        await Expect(Page.Locator("h1")).ToContainTextAsync("Account Settings");
    }

    // ─────────────────────────────────────────────────
    // V1.2 — Navigation via UserMenu (skipped — component not integrated yet)
    // ─────────────────────────────────────────────────

    [Test]
    [Ignore("UserMenu component not yet integrated into app layout")]
    public async Task NavigateToAccount_FromUserMenu_ShowsAccountPage()
    {
        await Page.Locator("[data-testid=\"auth-user-menu-trigger\"]").ClickAsync();
        await Page.WaitForSelectorAsync("[data-testid=\"auth-user-menu-dropdown\"]");
        await Page.Locator("[data-testid=\"auth-user-menu-settings\"]").ClickAsync();
        await Expect(Page).ToHaveURLAsync(Url("/account"));
    }

    // ─────────────────────────────────────────────────
    // V1.3 — Page Shell Structure
    // ─────────────────────────────────────────────────

    [Test]
    public async Task AccountPage_Renders_ShellWithHeaderAndCard()
    {
        await Page.GotoAsync(Url("/account"));
        await Page.WaitForSelectorAsync("[data-testid=\"account-profile-card\"]");

        await Expect(Page.Locator("h1")).ToHaveTextAsync("Account Settings");
        await Expect(Page.Locator("[data-testid=\"account-settings-subtitle\"]"))
            .ToContainTextAsync("Manage your profile");
        await Expect(Page.Locator("[data-testid=\"account-profile-card\"]"))
            .ToBeVisibleAsync();
    }

    // ─────────────────────────────────────────────────
    // V1.4 — Profile Card Read Mode
    // ─────────────────────────────────────────────────

    [Test]
    public async Task ProfileCard_ReadMode_ShowsAllFields()
    {
        await Page.GotoAsync(Url("/account"));
        await Page.WaitForSelectorAsync("[data-testid=\"account-profile-card\"]");

        var card = Page.Locator("[data-testid=\"account-profile-card\"]");

        await Expect(card.Locator("[data-testid=\"profile-display-name-text\"]"))
            .ToBeVisibleAsync();
        await Expect(card.Locator("[data-testid=\"profile-email-text\"]"))
            .ToBeVisibleAsync();

        var memberSince = card.Locator("[data-testid=\"profile-member-since-text\"]");
        await Expect(memberSince).ToBeVisibleAsync();
        var text = await memberSince.TextContentAsync();
        Assert.That(text, Does.Contain("202"));

        await Expect(card.Locator("[data-testid=\"profile-edit-button\"]"))
            .ToBeVisibleAsync();
    }

    // ─────────────────────────────────────────────────
    // V1.5 — Profile Card Edit Mode Activation
    // ─────────────────────────────────────────────────

    [Test]
    public async Task ProfileCard_ClickEdit_EntersEditMode()
    {
        await Page.GotoAsync(Url("/account"));
        await Page.WaitForSelectorAsync("[data-testid=\"profile-edit-button\"]");

        await Page.Locator("[data-testid=\"profile-edit-button\"]").ClickAsync();
        await Page.WaitForSelectorAsync("[data-testid=\"profile-display-name-input\"]");

        var card = Page.Locator("[data-testid=\"account-profile-card\"]");

        await Expect(card.Locator("[data-testid=\"profile-display-name-input\"]"))
            .ToBeVisibleAsync();
        await Expect(card.Locator("[data-testid=\"profile-display-name-input\"]"))
            .Not.ToBeEmptyAsync();
        await Expect(card.Locator("[data-testid=\"profile-save-button\"]"))
            .ToBeVisibleAsync();
        await Expect(card.Locator("[data-testid=\"profile-cancel-button\"]"))
            .ToBeVisibleAsync();
        await Expect(card.Locator("[data-testid=\"profile-edit-button\"]"))
            .ToBeHiddenAsync();
    }

    // ─────────────────────────────────────────────────
    // V1.6 — Profile Edit — Save
    // ─────────────────────────────────────────────────

    [Test]
    public async Task ProfileCard_SaveEdit_UpdatesDisplayName()
    {
        await Page.GotoAsync(Url("/account"));
        await Page.WaitForSelectorAsync("[data-testid=\"profile-edit-button\"]");

        await Page.Locator("[data-testid=\"profile-edit-button\"]").ClickAsync();
        await Page.WaitForSelectorAsync("[data-testid=\"profile-display-name-input\"]");

        var input = Page.Locator("[data-testid=\"profile-display-name-input\"]");
        await input.FillAsync("Alice Brown");
        await Page.Locator("[data-testid=\"profile-save-button\"]").ClickAsync();

        await Page.WaitForSelectorAsync("[data-testid=\"profile-display-name-text\"]");

        await Expect(Page.Locator("[data-testid=\"profile-display-name-text\"]"))
            .ToHaveTextAsync("Alice Brown");
        await Expect(Page.Locator("[data-testid=\"profile-edit-button\"]"))
            .ToBeVisibleAsync();
    }

    // ─────────────────────────────────────────────────
    // V1.7 — Profile Edit — Cancel
    // ─────────────────────────────────────────────────

    [Test]
    public async Task ProfileCard_CancelEdit_RevertsToOriginal()
    {
        await Page.GotoAsync(Url("/account"));
        await Page.WaitForSelectorAsync("[data-testid=\"profile-edit-button\"]");

        var originalName = await Page.Locator("[data-testid=\"profile-display-name-text\"]")
            .TextContentAsync();

        await Page.Locator("[data-testid=\"profile-edit-button\"]").ClickAsync();
        await Page.WaitForSelectorAsync("[data-testid=\"profile-display-name-input\"]");

        await Page.Locator("[data-testid=\"profile-display-name-input\"]")
            .FillAsync("Changed Name");
        await Page.Locator("[data-testid=\"profile-cancel-button\"]").ClickAsync();

        await Page.WaitForSelectorAsync("[data-testid=\"profile-display-name-text\"]");

        await Expect(Page.Locator("[data-testid=\"profile-display-name-text\"]"))
            .ToHaveTextAsync(originalName!);
    }

    // ─────────────────────────────────────────────────
    // V1.8 — Profile Edit — Empty Validation
    // ─────────────────────────────────────────────────

    [Test]
    public async Task ProfileCard_SaveEmpty_ShowsValidationError()
    {
        await Page.GotoAsync(Url("/account"));
        await Page.WaitForSelectorAsync("[data-testid=\"profile-edit-button\"]");

        await Page.Locator("[data-testid=\"profile-edit-button\"]").ClickAsync();
        await Page.WaitForSelectorAsync("[data-testid=\"profile-display-name-input\"]");

        await Page.Locator("[data-testid=\"profile-display-name-input\"]").FillAsync("");
        await Page.Locator("[data-testid=\"profile-save-button\"]").ClickAsync();

        // Wait for client-side validation error to appear (no network call needed)
        await Expect(Page.Locator("[data-testid=\"profile-display-name-error\"]"))
            .ToBeVisibleAsync();
        await Expect(Page.Locator("[data-testid=\"profile-display-name-input\"]"))
            .ToBeVisibleAsync();
    }

    // ─────────────────────────────────────────────────
    // V1.9 — Profile Edit — Max Length Validation
    // ─────────────────────────────────────────────────

    [Test]
    public async Task ProfileCard_SaveTooLong_ShowsValidationError()
    {
        await Page.GotoAsync(Url("/account"));
        await Page.WaitForSelectorAsync("[data-testid=\"profile-edit-button\"]");

        await Page.Locator("[data-testid=\"profile-edit-button\"]").ClickAsync();
        await Page.WaitForSelectorAsync("[data-testid=\"profile-display-name-input\"]");

        var longName = new string('a', 101);
        await Page.Locator("[data-testid=\"profile-display-name-input\"]").FillAsync(longName);
        await Page.Locator("[data-testid=\"profile-save-button\"]").ClickAsync();

        // Wait for client-side validation error to appear
        await Expect(Page.Locator("[data-testid=\"profile-display-name-error\"]"))
            .ToBeVisibleAsync();
    }

    // ─────────────────────────────────────────────────
    // V1.10 — Profile Edit — Enter to Save
    // ─────────────────────────────────────────────────

    [Test]
    public async Task ProfileCard_PressEnter_SavesName()
    {
        await Page.GotoAsync(Url("/account"));
        await Page.WaitForSelectorAsync("[data-testid=\"profile-edit-button\"]");

        await Page.Locator("[data-testid=\"profile-edit-button\"]").ClickAsync();
        await Page.WaitForSelectorAsync("[data-testid=\"profile-display-name-input\"]");

        var input = Page.Locator("[data-testid=\"profile-display-name-input\"]");
        await input.FillAsync("Enter Saved");
        await input.PressAsync("Enter");

        await Page.WaitForSelectorAsync("[data-testid=\"profile-display-name-text\"]");

        await Expect(Page.Locator("[data-testid=\"profile-display-name-text\"]"))
            .ToHaveTextAsync("Enter Saved");
        await Expect(Page.Locator("[data-testid=\"profile-edit-button\"]"))
            .ToBeVisibleAsync();
    }

    // ─────────────────────────────────────────────────
    // V1.11 — Profile Edit — Escape to Cancel
    // ─────────────────────────────────────────────────

    [Test]
    public async Task ProfileCard_PressEscape_RevertsToOriginal()
    {
        await Page.GotoAsync(Url("/account"));
        await Page.WaitForSelectorAsync("[data-testid=\"profile-edit-button\"]");

        var originalName = await Page.Locator("[data-testid=\"profile-display-name-text\"]")
            .TextContentAsync();

        await Page.Locator("[data-testid=\"profile-edit-button\"]").ClickAsync();
        await Page.WaitForSelectorAsync("[data-testid=\"profile-display-name-input\"]");

        await Page.Locator("[data-testid=\"profile-display-name-input\"]")
            .FillAsync("Should Revert");
        await Page.Locator("[data-testid=\"profile-display-name-input\"]")
            .PressAsync("Escape");

        await Page.WaitForSelectorAsync("[data-testid=\"profile-display-name-text\"]");

        await Expect(Page.Locator("[data-testid=\"profile-display-name-text\"]"))
            .ToHaveTextAsync(originalName!);
    }

    // ─────────────────────────────────────────────────
    // V1.12 — Responsive — Mobile (375px)
    // ─────────────────────────────────────────────────

    [Test]
    public async Task AccountPage_MobileViewport_RendersStackedLayout()
    {
        await Page.SetViewportSizeAsync(375, 812);

        await Page.GotoAsync(Url("/account"));
        await Page.WaitForSelectorAsync("[data-testid=\"account-profile-card\"]");

        await Expect(Page.Locator("h1")).ToBeVisibleAsync();
        await Expect(Page.Locator("[data-testid=\"account-profile-card\"]"))
            .ToBeVisibleAsync();

        var bodyWidth = await Page.EvaluateAsync<int>("document.body.scrollWidth");
        Assert.That(bodyWidth, Is.LessThanOrEqualTo(375));
    }

    // ─────────────────────────────────────────────────
    // V1.13 — Responsive — Tablet collapsed sidebar (768px)
    // ─────────────────────────────────────────────────

    [Test]
    public async Task AccountPage_TabletViewport_ShowsCompactSidebar()
    {
        await Page.SetViewportSizeAsync(768, 1024);

        await Page.GotoAsync(Url("/account"));
        await Page.WaitForSelectorAsync("[data-testid=\"account-profile-card\"]");

        var sidebar = Page.Locator("[data-testid=\"sidebar\"]");
        await Expect(sidebar).ToBeVisibleAsync();
        await Expect(Page.Locator("[data-testid=\"account-profile-card\"]"))
            .ToBeVisibleAsync();
    }

    // ═════════════════════════════════════════════════
    // Phase 2: Linked Providers
    // ═════════════════════════════════════════════════

    // ─────────────────────────────────────────────────
    // V2.1 — Providers card renders
    // ─────────────────────────────────────────────────

    [Test]
    public async Task ProvidersCard_Renders_WithHeaderAndList()
    {
        await Page.GotoAsync(Url("/account"));
        await Page.WaitForSelectorAsync("[data-testid=\"linked-providers-card\"]");

        // Card header
        await Expect(Page.Locator("[data-testid=\"linked-providers-card\"] h2"))
            .ToContainTextAsync("Linked Providers");

        // Link New button
        await Expect(Page.Locator("[data-testid=\"link-new-provider-button\"]"))
            .ToBeVisibleAsync();

        // Provider rows visible
        await Expect(Page.Locator("[data-testid=\"providers-list\"]"))
            .ToBeVisibleAsync();
    }

    // ─────────────────────────────────────────────────
    // V2.2 — Provider rows display correct data
    // ─────────────────────────────────────────────────

    [Test]
    public async Task ProviderRows_Display_ProviderInfo()
    {
        await Page.GotoAsync(Url("/account"));
        await Page.WaitForSelectorAsync("[data-testid=\"provider-row\"]");

        var rows = Page.Locator("[data-testid=\"provider-row\"]");
        var count = await rows.CountAsync();
        Assert.That(count, Is.GreaterThanOrEqualTo(2));

        // First row should be Google
        var firstRow = rows.Nth(0);
        await Expect(firstRow).ToContainTextAsync("Google");
        await Expect(firstRow).ToContainTextAsync("Active");

        // Second row should be GitHub
        var secondRow = rows.Nth(1);
        await Expect(secondRow).ToContainTextAsync("GitHub");
        await Expect(secondRow).ToContainTextAsync("Active");
    }

    // ─────────────────────────────────────────────────
    // V2.3 — Unlink button enabled with multiple providers
    // ─────────────────────────────────────────────────

    [Test]
    public async Task UnlinkButton_Enabled_WithMultipleProviders()
    {
        await Page.GotoAsync(Url("/account"));
        await Page.WaitForSelectorAsync("[data-testid=\"provider-row\"]");

        var unlinkButtons = Page.Locator("[data-testid=\"provider-unlink-button\"]");
        var count = await unlinkButtons.CountAsync();

        // Both should be enabled when we have 2+ providers
        for (var i = 0; i < count; i++)
        {
            await Expect(unlinkButtons.Nth(i)).ToBeEnabledAsync();
        }
    }

    // ─────────────────────────────────────────────────
    // V2.4 — Unlink removes a provider
    // ─────────────────────────────────────────────────

    [Test]
    public async Task UnlinkProvider_RemovesRow_FromList()
    {
        await Page.GotoAsync(Url("/account"));
        await Page.WaitForSelectorAsync("[data-testid=\"provider-row\"]");

        var initialCount = await Page.Locator("[data-testid=\"provider-row\"]").CountAsync();

        // Click Unlink on the first provider row
        var firstUnlink = Page.Locator("[data-testid=\"provider-unlink-button\"]").Nth(0);
        await firstUnlink.ClickAsync();

        // Auto-wait for the row count to decrease
        await Expect(Page.Locator("[data-testid=\"provider-row\"]"))
            .ToHaveCountAsync(initialCount - 1);
    }

    // ─────────────────────────────────────────────────
    // V2.5 — Unlink disabled when only one provider remains
    // ─────────────────────────────────────────────────

    [Test]
    public async Task UnlinkButton_Disabled_WhenOnlyOneProvider()
    {
        await Page.GotoAsync(Url("/account"));
        await Page.WaitForSelectorAsync("[data-testid=\"provider-row\"]");

        // Unlink both providers to get down to 1 (the stub has 2)
        var unlinkButtons = Page.Locator("[data-testid=\"provider-unlink-button\"]");
        var initialCount = await unlinkButtons.CountAsync();

        // Click first unlink
        await unlinkButtons.Nth(0).ClickAsync();

        // Auto-wait for count to decrease
        await Expect(Page.Locator("[data-testid=\"provider-row\"]"))
            .ToHaveCountAsync(initialCount - 1);

        var remainingCount = initialCount - 1;

        if (remainingCount == 1)
        {
            await Expect(Page.Locator("[data-testid=\"provider-unlink-button\"]"))
                .ToBeDisabledAsync();
        }
    }

    // ─────────────────────────────────────────────────
    // V2.6 — Link New button exists
    // ─────────────────────────────────────────────────

    [Test]
    public async Task LinkNewButton_IsVisible_InProviderCard()
    {
        await Page.GotoAsync(Url("/account"));
        await Page.WaitForSelectorAsync("[data-testid=\"link-new-provider-button\"]");

        await Expect(Page.Locator("[data-testid=\"link-new-provider-button\"]"))
            .ToBeVisibleAsync();
        await Expect(Page.Locator("[data-testid=\"link-new-provider-button\"]"))
            .ToContainTextAsync("Link New");
    }

    // ═════════════════════════════════════════════════
    // Phase 3: Document Sources
    // ═════════════════════════════════════════════════

    // ─────────────────────────────────────────────────
    // V3.1 — Sources card renders
    // ─────────────────────────────────────────────────

    [Test]
    public async Task SourcesCard_Renders_WithHeaderAndList()
    {
        await Page.GotoAsync(Url("/account"));
        await Page.WaitForSelectorAsync("[data-testid=\"document-sources-card\"]");

        await Expect(Page.Locator("[data-testid=\"document-sources-card\"] h2"))
            .ToContainTextAsync("Document Sources");

        await Expect(Page.Locator("[data-testid=\"add-source-button\"]"))
            .ToBeVisibleAsync();

        await Expect(Page.Locator("[data-testid=\"sources-list\"]"))
            .ToBeVisibleAsync();
    }

    // ─────────────────────────────────────────────────
    // V3.2 — Source rows display correct data
    // ─────────────────────────────────────────────────

    [Test]
    public async Task SourceRows_Display_SourceInfo()
    {
        await Page.GotoAsync(Url("/account"));
        await Page.WaitForSelectorAsync("[data-testid=\"source-row\"]");

        var rows = Page.Locator("[data-testid=\"source-row\"]");
        var count = await rows.CountAsync();
        Assert.That(count, Is.GreaterThanOrEqualTo(2));

        // First row: Biology Decks, Google Drive, Active
        var firstRow = rows.Nth(0);
        await Expect(firstRow).ToContainTextAsync("Biology Decks");
        await Expect(firstRow).ToContainTextAsync("Google Drive");
        await Expect(firstRow).ToContainTextAsync("Active");

        // Second row: CS Notes, GitHub, Error
        var secondRow = rows.Nth(1);
        await Expect(secondRow).ToContainTextAsync("CS Notes");
        await Expect(secondRow).ToContainTextAsync("GitHub");
        await Expect(secondRow).ToContainTextAsync("Error");
    }

    // ─────────────────────────────────────────────────
    // V3.3 — Inline rename
    // ─────────────────────────────────────────────────

    [Test]
    public async Task SourceRow_InlineRename_UpdatesName()
    {
        await Page.GotoAsync(Url("/account"));
        await Page.WaitForSelectorAsync("[data-testid=\"source-edit-button\"]");

        // Click Edit on first source row
        await Page.Locator("[data-testid=\"source-edit-button\"]").Nth(0).ClickAsync();
        await Page.WaitForSelectorAsync("[data-testid=\"source-name-input\"]");

        // Type new name
        var input = Page.Locator("[data-testid=\"source-name-input\"]");
        await input.FillAsync("Advanced Biology");
        await Page.Locator("[data-testid=\"source-save-button\"]").ClickAsync();

        // Auto-wait for name text to update after save
        await Expect(Page.Locator("[data-testid=\"source-name-text\"]").Nth(0))
            .ToHaveTextAsync("Advanced Biology");
    }

    // ─────────────────────────────────────────────────
    // V3.4 — Remove source
    // ─────────────────────────────────────────────────

    [Test]
    public async Task SourceRow_Remove_RemovesRow()
    {
        await Page.GotoAsync(Url("/account"));
        await Page.WaitForSelectorAsync("[data-testid=\"source-row\"]");

        var initialCount = await Page.Locator("[data-testid=\"source-row\"]").CountAsync();

        // Click Remove on first source
        await Page.Locator("[data-testid=\"source-remove-button\"]").Nth(0).ClickAsync();

        // Auto-wait for the row count to decrease
        await Expect(Page.Locator("[data-testid=\"source-row\"]"))
            .ToHaveCountAsync(initialCount - 1);
    }

    // ─────────────────────────────────────────────────
    // V3.5 — Add button exists
    // ─────────────────────────────────────────────────

    [Test]
    public async Task AddSourceButton_IsVisible_InSourcesCard()
    {
        await Page.GotoAsync(Url("/account"));
        await Page.WaitForSelectorAsync("[data-testid=\"add-source-button\"]");

        await Expect(Page.Locator("[data-testid=\"add-source-button\"]"))
            .ToBeVisibleAsync();
        await Expect(Page.Locator("[data-testid=\"add-source-button\"]"))
            .ToContainTextAsync("Add");
    }

    // ═════════════════════════════════════════════════
    // Phase 4: Account Deletion (Danger Zone)
    // ═════════════════════════════════════════════════

    // ─────────────────────────────────────────────────
    // V4.1 — Danger zone card renders
    // ─────────────────────────────────────────────────

    [Test]
    public async Task DangerZoneCard_Renders_WithDeleteButton()
    {
        await Page.GotoAsync(Url("/account"));
        await Page.WaitForSelectorAsync("[data-testid=\"danger-zone-card\"]");

        await Expect(Page.Locator("[data-testid=\"danger-zone-card\"] h2"))
            .ToContainTextAsync("Danger Zone");

        await Expect(Page.Locator("[data-testid=\"danger-zone-card\"]"))
            .ToContainTextAsync("This action cannot be undone");

        await Expect(Page.Locator("[data-testid=\"delete-account-button\"]"))
            .ToBeVisibleAsync();
    }

    // ─────────────────────────────────────────────────
    // V4.2 — Delete button opens confirmation dialog
    // ─────────────────────────────────────────────────

    [Test]
    public async Task DeleteButton_Opens_ConfirmationDialog()
    {
        await Page.GotoAsync(Url("/account"));
        await Page.WaitForSelectorAsync("[data-testid=\"delete-account-button\"]");

        await Page.Locator("[data-testid=\"delete-account-button\"]").ClickAsync();
        await Page.WaitForSelectorAsync("[data-testid=\"delete-dialog\"]");

        await Expect(Page.Locator("[data-testid=\"delete-dialog\"]"))
            .ToContainTextAsync("Delete Account");
        await Expect(Page.Locator("[data-testid=\"delete-dialog-warning\"]"))
            .ToContainTextAsync("This action CANNOT be undone");
        await Expect(Page.Locator("[data-testid=\"delete-confirm-email-input\"]"))
            .ToBeVisibleAsync();
        await Expect(Page.Locator("[data-testid=\"delete-confirm-button\"]"))
            .ToBeDisabledAsync();
        await Expect(Page.Locator("[data-testid=\"delete-cancel-button\"]"))
            .ToBeVisibleAsync();
    }

    // ─────────────────────────────────────────────────
    // V4.3 — Confirm button enables on correct email
    // ─────────────────────────────────────────────────

    [Test]
    public async Task ConfirmButton_Enables_WhenEmailMatches()
    {
        await Page.GotoAsync(Url("/account"));
        await Page.WaitForSelectorAsync("[data-testid=\"delete-account-button\"]");

        await Page.Locator("[data-testid=\"delete-account-button\"]").ClickAsync();
        await Page.WaitForSelectorAsync("[data-testid=\"delete-dialog\"]");

        // Type wrong email — button stays disabled
        var input = Page.Locator("[data-testid=\"delete-confirm-email-input\"]");
        await input.FillAsync("wrong@email.com");
        await Expect(Page.Locator("[data-testid=\"delete-confirm-button\"]"))
            .ToBeDisabledAsync();
        await Expect(Page.Locator("[data-testid=\"delete-email-mismatch\"]"))
            .ToBeVisibleAsync();

        // Type correct email — button enables
        await input.FillAsync("student@mnemi.app");
        await Expect(Page.Locator("[data-testid=\"delete-confirm-button\"]"))
            .ToBeEnabledAsync();
    }

    // ─────────────────────────────────────────────────
    // V4.4 — Cancel closes dialog
    // ─────────────────────────────────────────────────

    [Test]
    public async Task CancelButton_Closes_ConfirmationDialog()
    {
        await Page.GotoAsync(Url("/account"));
        await Page.WaitForSelectorAsync("[data-testid=\"delete-account-button\"]");

        await Page.Locator("[data-testid=\"delete-account-button\"]").ClickAsync();
        await Page.WaitForSelectorAsync("[data-testid=\"delete-dialog\"]");

        await Page.Locator("[data-testid=\"delete-cancel-button\"]").ClickAsync();

        // Dialog should be hidden after cancel
        await Expect(Page.Locator("[data-testid=\"delete-dialog\"]"))
            .ToBeHiddenAsync();
    }

    // ─────────────────────────────────────────────────
    // V4.5 — Click overlay closes dialog
    // ─────────────────────────────────────────────────

    [Test]
    public async Task ClickOverlay_Closes_ConfirmationDialog()
    {
        await Page.GotoAsync(Url("/account"));
        await Page.WaitForSelectorAsync("[data-testid=\"delete-account-button\"]");

        await Page.Locator("[data-testid=\"delete-account-button\"]").ClickAsync();
        await Page.WaitForSelectorAsync("[data-testid=\"delete-dialog\"]");

        // Click the overlay (outside the dialog center)
        await Page.Locator("[data-testid=\"delete-dialog-overlay\"]").ClickAsync(new LocatorClickOptions { Position = new Position { X = 10, Y = 10 } });

        // Dialog should be hidden after clicking overlay
        await Expect(Page.Locator("[data-testid=\"delete-dialog\"]"))
            .ToBeHiddenAsync();
    }

    // ─────────────────────────────────────────────────
    // V4.6 — Delete account navigates to login
    // ─────────────────────────────────────────────────

    [Test]
    public async Task DeleteAccount_Confirms_NavigatesToLogin()
    {
        await Page.GotoAsync(Url("/account"));
        await Page.WaitForSelectorAsync("[data-testid=\"delete-account-button\"]");

        await Page.Locator("[data-testid=\"delete-account-button\"]").ClickAsync();
        await Page.WaitForSelectorAsync("[data-testid=\"delete-dialog\"]");

        // Type correct email
        await Page.Locator("[data-testid=\"delete-confirm-email-input\"]")
            .FillAsync("student@mnemi.app");

        // Confirm deletion
        await Page.Locator("[data-testid=\"delete-confirm-button\"]").ClickAsync();

        // Should navigate to /login?deleted=true
        await Expect(Page).ToHaveURLAsync(Url("/login?deleted=true"));
    }
}
