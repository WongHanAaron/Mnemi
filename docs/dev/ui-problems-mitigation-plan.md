# UI Problems Mitigation Plan — Sidebar, CSS Architecture & Page Layout

> Phased remediation plan for all 16 issues identified in `docs/users/ui-problems-research.md`.
> Phases are ordered from **least impact** (zero behavioral change) to **most impact** (architectural shifts).
> Each phase includes an **Agent Verification Set** — a checklist of verifiable actions an agent can perform via MCP tools.

---

## Phase 1: Zero-Impact Cleanup

**Goal:** Fix bugs and improve code quality with zero behavioral changes. No user-visible effects. No API surface changes.

### 1.0 → Fix CSS Syntax Error (Stray `d` Character)

**Status:** 🔴 **Critical — was missed by original analysis**

**Problem:** A stray character `d` precedes a CSS rule at line ~390, causing the entire rule to be ignored by browsers. This breaks the pin icon sizing in compact sidebar mode on tablet viewports.

**File:** `src/App/Ui.Shared/wwwroot/Styles/sidebar-layout.css` (line ~390)

**Code reference:**
```css
d    .app-sidebar--open .sidebar-section__pinicon {
    width: 0.875rem;
    height: 0.875rem;
}
```

**Changes:**
Remove the stray `d` character:

```css
    .app-sidebar--open .sidebar-section__pinicon {
        width: 0.875rem;
        height: 0.875rem;
    }
```

**Verification:** Grep `sidebar-layout.css` for `^d\s+\.`. Should return zero results.

---

### 1.1 → Replace `max-height` Animation with `grid-template-rows`

**Problem:** `max-height` animation triggers layout recalculation and requires a guessed value (`50rem`).

**Files to change:**
- `src/App/Ui.Shared/wwwroot/Styles/sidebar-layout.css`

**Changes:**
Replace the `max-height` animation pattern with the modern `grid-template-rows` approach:

```css
/* Before */
.sidebar-section__content {
    overflow: hidden;
    max-height: 0;
    opacity: 0;
    transition: max-height 0.25s ease, opacity 0.2s ease, margin 0.25s ease;
    margin-top: 0;
}

.sidebar-section__content--open {
    max-height: 50rem;
    opacity: 1;
    margin-top: 0.25rem;
}

/* After */
.sidebar-section__content {
    display: grid;
    grid-template-rows: 0fr;
    opacity: 0;
    transition: grid-template-rows 0.25s ease, opacity 0.2s ease, margin 0.25s ease;
    margin-top: 0;
}

.sidebar-section__content--open {
    grid-template-rows: 1fr;
    opacity: 1;
    margin-top: 0.25rem;
}

/* Inner wrapper needed for overflow */
.sidebar-section__content > :first-child {
    overflow: hidden;
}
```

**Verification:** Toggle a collapsible section in the browser. The animation should open and close smoothly with no clipping.

---

### Phase 1 Verification Set (Agent-Verifiable)

```
[ ] 1.0  Grep sidebar-layout.css for "^d\\s+\\.". Should return zero results (stray character removed).
[ ] 1.1  Read sidebar-layout.css lines 136–143. Verify max-height is replaced with grid-template-rows pattern.
[ ] 1.1  Verify a new rule ".sidebar-section__content > :first-child { overflow: hidden; }" exists.
```

### Phase 1 Manual UI Verification (Human-Performed)

> Open the app in a browser (`dotnet run --project src/Ui.Web/Ui.Web.csproj --launch-profile Http`) and perform these checks.

| # | Check | Steps | Expected Result |
|---|-------|-------|-----------------|
| 1.0 | Pin icon sizing on tablet | Resize browser to 768–1023px viewport. Open sidebar. Observe pin icons in collapsed sections. | Pin icons render at correct size (0.875rem). No visual distortion. |
| 1.1 | Section collapse/expand animation | Click a collapsible section header to collapse, then expand again. Observe the animation. | Smooth open/close animation with no content clipping, no layout jump, no `max-height` guess value artifacts. |
| 1.1 | Animation performance | Open DevTools Performance tab. Record a collapse/expand cycle. | No forced synchronous layouts. No layout thrashing during transition. |
| 1.0 | Build verification | Run `dotnet build src/Ui.Web/Ui.Web.csproj`. | Clean build with zero errors and zero warnings. |

---

## Phase 2: Low-Impact Fixes

**Goal:** Improve accessibility, remove redundancy, and fix minor UX issues. No API surface changes. No new services or dependencies.

### 1.1 → Remove Duplicate CSS Definitions from app.css

**Problem:** `app.css` re-declares `.layout-shell` and `.layout-content` with incomplete definitions.

**Files to change:**
- `src/App/Ui.Web/wwwroot/css/app.css`

**Changes:**
Remove the duplicate `.layout-shell` and `.layout-content` rules from `app.css`. The authoritative definitions live in `sidebar-layout.css` which is loaded via the RCL content path.

```css
/* Remove these lines from app.css */
.layout-shell {
    /* Defined in Ui.Shared wwwroot/Styles/sidebar-layout.css */
    min-height: 100vh;
}

.layout-content {
    /* Defined in Ui.Shared wwwroot/Styles/sidebar-layout.css */
    min-height: 100vh;
}
```

**Verification:** Grep `app.css` for `.layout-shell` and `.layout-content`. Neither should appear as a CSS rule (comments are fine).

---

### 1.2 → Remove Duplicate Stylesheet Link

**Problem:** `sidebar-layout.css` is linked in both `MainLayout.razor` and `App.razor`.

**Files to change:**
- `src/App/Ui.Web/Components/App.razor`

**Changes:**
Remove the `<link>` tag from `App.razor`. Keep it only in `MainLayout.razor` since that is the layout that wraps all authenticated pages and is the canonical place for layout-level assets.

```razor
<!-- Remove this line from App.razor -->
<link rel="stylesheet" href="_content/Ui.Shared/Styles/sidebar-layout.css" />
```

**Verification:** Grep the entire workspace for `sidebar-layout.css`. It should appear in exactly one `<link>` tag (in `MainLayout.razor`).

---

### 4.1 → Add `:focus-visible` Styles to Sidebar Toggle

**Problem:** No visible focus indicator on the sidebar toggle button.

**Files to change:**
- `src/App/Ui.Shared/wwwroot/Styles/sidebar-layout.css`

**Changes:**
Add focus-visible styles inside the `@media (max-width: 1023px)` block:

```css
.mnemi-sidebar-toggle:focus-visible {
    outline: 2px solid var(--sidebar-ring, #6b6b6b);
    outline-offset: 2px;
}
```

**Verification:** In a browser, tab to the sidebar toggle button. A visible ring should appear around it.

---

### 4.3 → Use Semantic HTML for Quick Links (Audit Finding #5, #10)

**Problem:** Navigation links are not announced as a list by screen readers.

**Audit correction:** The original recommendation to add `role="list"` and `role="listitem"` is a workaround. The **better approach** is to use native semantic HTML, which provides list semantics without ARIA augmentation.

**Files to change:**
- `src/App/Ui.Shared/Layout/SidebarQuickLinks.razor`

**Changes:**
Replace the `<div>` container with a `<nav>` and use `<ul>`/`<li>` for list semantics:

```razor
<nav class="mnemi-sidebar-quick-links" aria-label="Quick navigation" data-testid="sidebar-quick-links">
    <ul style="list-style: none; margin: 0; padding: 0;">
        @foreach (var item in QuickLinks)
        {
            <li>
                <Ui.Shared.Components.Layout.SidebarNavLink ... />
            </li>
        }
    </ul>
</nav>
```

**Why this is better:**
- `<nav>` provides native landmark semantics for navigation regions
- `<ul>`/`<li>` provide native list semantics — no ARIA roles needed
- Screen readers announce "navigation list" automatically
- Less markup, better accessibility, no ARIA duplication

**Verification:** Run a browser accessibility audit (Lighthouse). The "Links do not share a focusable parent" warning should be resolved. The page should have a "Navigation" landmark.

---

### 3.3 → Add `prefers-reduced-motion` Support

**Problem:** No support for users who prefer reduced motion.

**Files to change:**
- `src/App/Ui.Shared/wwwroot/Styles/sidebar-layout.css`

**Changes:**
Add a `prefers-reduced-motion` media query at the end of the file:

```css
@media (prefers-reduced-motion: reduce) {
    .mnemi-sidebar,
    .mnemi-sidebar--open,
    .mnemi-sidebar-backdrop,
    .mnemi-sidebar-backdrop--visible,
    .mnemi-sidebar-section__content,
    .mnemi-sidebar-section__content--open,
    .mnemi-sidebar-nav-link,
    .mnemi-sidebar-section__header,
    .mnemi-sidebar-toggle {
        transition: none !important;
    }
}
```

**Verification:** In browser DevTools, emulate `prefers-reduced-motion: reduce`. All sidebar animations should be instant (no transition).

---

### 2.4 → Add `@bind-Expanded` Support to SidebarSection (Audit Finding #3)

**Problem:** `IsExpanded` state is internal-only, preventing parent control.

**Audit correction:** The original claim that there is "no external binding support" is incomplete. There **is** an `InitiallyExpanded` parameter that controls the starting state externally. What's missing is a two-way `[Parameter] public bool IsExpanded { get; set; }` for **runtime** control.

**Files to change:**
- `src/App/Ui.Shared/Layout/SidebarSection.razor`

**Changes:**
Add a two-way bindable parameter alongside the existing `InitiallyExpanded`:

```razor
/// <summary>
/// External expansion state. When null, the section manages its own state internally.
/// When set, enables two-way binding for parent-controlled expansion.
/// </summary>
[Parameter]
public bool? Expanded { get; set; }

[Parameter]
public EventCallback<bool> ExpandedChanged { get; set; }

private bool _internalExpanded = true;
private bool IsExpanded
{
    get => Expanded ?? _internalExpanded;
    set
    {
        if (Expanded.HasValue)
        {
            _internalExpanded = value;
            ExpandedChanged.InvokeAsync(value);
        }
        else
        {
            _internalExpanded = value;
        }
    }
}
```

**Verification:** Create a test page that passes `@bind-Expanded` to two `SidebarSection` components. Toggling one should not affect the other.

---

### Phase 2 Verification Set (Agent-Verifiable)

```
[ ] 1.1  Grep app.css for ".layout-shell {" and ".layout-content {". Neither should appear as CSS rules.
[ ] 1.2  Grep workspace for "sidebar-layout.css". Exactly one <link> tag should reference it (in MainLayout.razor).
[ ] 4.1  Read sidebar-layout.css. A ":focus-visible" rule targeting the sidebar toggle should exist with outline styling.
[ ] 4.3  Read SidebarQuickLinks.razor. The container should use <nav> with <ul>/<li> for native semantics (not role="list").
[ ] 3.3  Read sidebar-layout.css. A "@media (prefers-reduced-motion: reduce)" block should exist with transition: none.
[ ] 2.4  Read SidebarSection.razor. It should have [Parameter] bool? Expanded and EventCallback<bool> ExpandedChanged.
```

### Phase 2 Manual UI Verification (Human-Performed)

> Open the app in a browser and perform these checks.

| # | Check | Steps | Expected Result |
|---|-------|-------|-----------------|
| 1.1 | Duplicate CSS removal | Open DevTools Elements panel. Select the main layout shell. Inspect computed styles. | `.layout-shell` and `.layout-content` styles are sourced from `sidebar-layout.css` (check the stylesheet link in computed styles). No duplicate rules in `app.css`. |
| 1.2 | Single stylesheet load | Open DevTools Network tab. Reload the page. Filter by "css". | Only one request for `sidebar-layout.css`. No duplicate loads. |
| 4.1 | Focus indicator on toggle | Tab through the page using `Tab` key. Stop on the sidebar toggle button. | A visible focus ring (2px outline) appears around the toggle button. No outline on mouse click, only on keyboard tab. |
| 4.3 | Semantic navigation landmark | Open DevTools Accessibility Inspector (or use Lighthouse). Check the page landmarks. | "Navigation" landmark is present. Quick links are announced as a list within the navigation region. |
| 3.3 | Reduced motion support | In browser DevTools: `Rendering` panel → `Emulate CSS prefers-reduced-motion: reduce`. Reload page. Toggle sidebar, expand/collapse sections. | All animations are instant (no transition). Sidebar slides open/closed without animation. No motion artifacts. |
| 2.4 | Two-way binding on SidebarSection | Create a test page with two `SidebarSection` components using `@bind-Expanded`. Toggle one section. | Only the toggled section changes state. The other section remains unaffected. No state leakage. |

---

## Phase 3: Medium-Impact Refactors

**Goal:** Improve reusability, testability, and UX. Component parameters change but defaults maintain backward compatibility. No new services or dependencies.

### 2.2 → Accept Quick Links as a Parameter

**Problem:** `QuickLinks` is hardcoded as a `static readonly` field.

**Files to change:**
- `src/App/Ui.Shared/Layout/SidebarQuickLinks.razor`

**Changes:**
Replace the static field with a parameter that has a sensible default:

```razor
/// <summary>
/// Navigation links to display. Defaults to Home, Review, Decks if not provided.
/// </summary>
[Parameter]
public IReadOnlyList<SidebarNavItem> QuickLinks { get; set; } = null!;

protected override void OnInitialized()
{
    if (QuickLinks is null or { Count: 0 })
    {
        QuickLinks = new[]
        {
            new SidebarNavItem("home", "Home", "/", "home"),
            new SidebarNavItem("review", "Review", "/review", "review"),
            new SidebarNavItem("decks", "Decks", "/decks", "decks"),
        };
    }
}
```

**Verification:** The sidebar should render identically to before. Then create a test page that passes a custom list of 5 links and verify all 5 render.

---

### 2.3 → Extract GetInitials to Shared Utility

**Problem:** `GetInitials` is duplicated in two components.

**Files to create:**
- `src/App/Ui.Shared/Utils/StringExtensions.cs` (or `src/App/Ui.Shared/Services/InitialsService.cs`)

**Files to change:**
- `src/App/Ui.Shared/Layout/SidebarUserSection.razor`
- `src/App/Ui.Shared/Layout/SidebarPinnedDecks.razor`

**Changes:**
Create a shared utility:

```csharp
// src/App/Ui.Shared/Utils/StringExtensions.cs
namespace Ui.Shared.Utils;

public static class StringExtensions
{
    public static string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "?";

        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
            return $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant();

        return name[..Math.Min(2, name.Length)].ToUpperInvariant();
    }
}
```

Then replace both component implementations with:

```razor
@using Ui.Shared.Utils
...
private string Initials => StringExtensions.GetInitials(UserName);
// or
<span class="pinned-decks__icon" aria-hidden="true">@StringExtensions.GetInitials(deck.Name)</span>
```

**Verification:** Grep for `GetInitials` in the Layout/ folder. It should appear zero times as a method definition. Both components should reference the shared utility.

---

### 3.1 → Fix Sidebar Sticky Positioning on Desktop

**Problem:** Sidebar scrolls with the page on desktop because `position: sticky` is not `position: fixed`.

**Files to change:**
- `src/App/Ui.Shared/wwwroot/Styles/sidebar-layout.css`

**Changes:**
On desktop (the default state), change the sidebar from `position: sticky` to `position: fixed`:

```css
/* Before (line ~40) */
.app-sidebar {
    display: flex;
    flex-direction: column;
    height: 100vh;
    position: sticky;  /* <-- changes to fixed on desktop */
    top: 0;
    ...
}

/* After */
.mnemi-sidebar {
    display: flex;
    flex-direction: column;
    height: 100vh;
    position: fixed;
    top: 0;
    left: 0;
    width: 16rem;
    ...
}

/* Adjust the shell to account for fixed sidebar */
.mnemi-layout-shell {
    display: grid;
    grid-template-columns: 16rem minmax(0, 1fr);
    min-height: 100vh;
}

/* Content area needs left offset on desktop */
.mnemi-layout-content {
    min-height: 100vh;
    overflow-y: auto;
    grid-column: 2;
    margin-left: 16rem;  /* accounts for fixed sidebar */
}
```

Then in the tablet media query, remove the `margin-left` since the sidebar is absolute there:

```css
@media (min-width: 768px) and (max-width: 1023px) {
    .mnemi-layout-shell {
        position: relative;
        grid-template-columns: 4rem minmax(0, 1fr);
    }

    .mnemi-layout-content {
        margin-left: 4rem;  /* matches compact sidebar width */
    }

    .mnemi-sidebar--open {
        position: absolute;  /* not fixed */
        ...
    }
}
```

**Verification:** Scroll a long page on desktop. The sidebar should remain visible and fixed in place. The content area should not be hidden behind the sidebar.

---

### 5.1 → Respect User Sidebar Preference on Desktop

**Problem:** Sidebar auto-opens on desktop viewport change, overriding user preference.

**Files to change:**
- `src/App/Ui.Shared/Layout/AppSidebar.razor`

**Changes:**
Only auto-close on phone; do not auto-open on desktop:

```razor
private void HandleViewStateChanged(ViewState newState)
{
    _currentViewState = newState;
    if (newState == ViewState.Phone)
    {
        _sidebarOpen = false;
    }
    // Remove the else-if block that auto-opens on desktop:
    // else if (newState == ViewState.Desktop) { _sidebarOpen = true; }
    InvokeAsync(StateHasChanged);
}
```

**Verification:** Close the sidebar on desktop, then resize the browser to phone width and back to desktop. The sidebar should remain closed after returning to desktop.

---

### 4.2 → Add Keyboard Activation to Pinned Deck Items

**Problem:** Pinned deck items are not keyboard-activatable.

**Files to change:**
- `src/App/Ui.Shared/Layout/SidebarPinnedDecks.razor`

**Changes:**
Add keyboard event handler and change `<li>` to a keyboard-accessible pattern:

```razor
<li class="mnemi-pinned-decks__item"
    data-testid="pinned-deck-@deck.Id"
    title="@deck.Name"
    role="button"
    tabindex="0"
    aria-label="@deck.Name"
    @onclick="() => OnDeckSelected.InvokeAsync(deck)"
    @onkeydown="@(async (KeyboardEventArgs e) =>
    {
        if (e.Key is "Enter" or " ")
        {
            e.PreventDefault();
            await OnDeckSelected.InvokeAsync(deck);
        }
    })"
    @onclick:prevent_default>
    ...
</li>
```

**Verification:** Tab through the pinned deck items using only the keyboard. Pressing Enter or Space on an item should navigate to the deck.

---

### Phase 3 Verification Set (Agent-Verifiable)

```
[ ] 2.2  Read SidebarQuickLinks.razor. QuickLinks should be a [Parameter], not a static field. Default values should be set in OnInitialized.
[ ] 2.3  Grep Layout/ folder for "private static string GetInitials". Should return zero results.
[ ] 2.3  Verify Ui.Shared/Utils/StringExtensions.cs exists and contains the GetInitials method.
[ ] 3.1  Read sidebar-layout.css. The default .mnemi-sidebar should use position: fixed (not sticky).
[ ] 3.1  Verify .mnemi-layout-content has margin-left: 16rem on desktop.
[ ] 5.1  Read AppSidebar.razor. The HandleViewStateChanged method should NOT set _sidebarOpen = true for ViewState.Desktop.
[ ] 4.2  Read SidebarPinnedDecks.razor. The <li> element should have tabindex="0", role="button", and @onkeydown handler.
```

### Phase 3 Manual UI Verification (Human-Performed)

> Open the app in a browser and perform these checks.

| # | Check | Steps | Expected Result |
|---|-------|-------|-----------------|
| 2.2 | Custom quick links rendering | Create a test page that passes a custom list of 5 links to `SidebarQuickLinks`. | All 5 custom links render. Default links are not shown. |
| 2.3 | Initials display consistency | Navigate to a page with a user section and a pinned deck list. Compare the initials rendered in both. | Initials are identical for the same input. No duplication of logic visible in markup. |
| 3.1 | Sidebar stays fixed on scroll | On desktop viewport, scroll a long page (use DevTools to add extra content). | Sidebar remains visible and fixed in place. Content does not slide behind it. No white gaps appear. |
| 3.1 | Tablet compact mode layout | Resize browser to 768–1023px. Verify sidebar is in compact (4rem) mode. Scroll the page. | Sidebar is 4rem wide. Content has correct left offset. No overlap between sidebar and content. |
| 5.1 | Sidebar preference persists | Close the sidebar on desktop. Resize to phone width, then back to desktop. | Sidebar remains closed after returning to desktop. User preference is respected. |
| 4.2 | Keyboard navigation on pinned decks | Tab through the sidebar using only the keyboard. Stop on a pinned deck item. Press Enter, then Space. | Both Enter and Space trigger deck selection. No other keys trigger navigation. Visual focus indicator is visible. |

---

## Phase 4: High-Impact Architectural Changes

**Goal:** Restructure components for long-term maintainability. Requires coordination across multiple files. May affect rendering output.

### 2.1 → Extract Icons to a Dedicated Component

**Problem:** Inline SVG paths in `SidebarNavLink` couple the component to specific icon shapes.

**Files to create:**
- `src/App/Ui.Shared/Components/Layout/SidebarIcon.razor`

**Files to change:**
- `src/App/Ui.Shared/Layout/SidebarNavLink.razor`

**Changes:**
Create a reusable icon component:

```razor
@* SidebarIcon.razor *@
@namespace Ui.Shared.Components.Layout

<svg class="@Class"
     xmlns="http://www.w3.org/2000/svg"
     viewBox="0 0 24 24"
     fill="none"
     stroke="currentColor"
     stroke-width="2"
     stroke-linecap="round"
     stroke-linejoin="round"
     aria-hidden="true">
    @IconContent
</svg>

@code {
    [Parameter] public string Class { get; set; } = "";

    [Parameter] public RenderFragment? IconContent { get; set; }
}
```

Then in `SidebarNavLink.razor`, replace the inline SVG:

```razor
<SidebarIcon Class="mnemi-sidebar-nav-link__icon">
    @IconKind switch
    {
        "home" => @<><path d="m3 9 9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z" /><polyline points="9 22 9 12 15 12 15 22" /></>,
        "review" => @<><path d="M2 3h6a4 4 0 0 1 4 4v14a3 3 0 0 0-3-3H2z" /><path d="M22 3h-6a4 4 0 0 0-4 4v14a3 3 0 0 1 3-3h7z" /></>,
        "decks" => @<><rect x="3" y="3" width="7" height="9" rx="1" /><rect x="14" y="3" width="7" height="5" rx="1" /><rect x="14" y="12" width="7" height="9" rx="1" /></>,
        _ => @<><circle cx="12" cy="12" r="10" /><line x1="12" y1="8" x2="12" y2="16" /><line x1="8" y1="12" x2="16" y2="12" /></>,
    }
</SidebarIcon>
```

**Verification:** The sidebar should render identically. The `SidebarNavLink.razor` file should be significantly shorter (under 50 lines).

---

### 5.2 → Inject Pinned Deck Data via Service

**Problem:** Sample data is hardcoded in the layout component.

**Files to create:**
- `src/App/Application/Features/PinnedDecks/IGetPinnedDecks.cs` (interface)
- `src/App/Application/Features/PinnedDecks/GetPinnedDecksHandler.cs` (handler)
- `src/App/Ui.Web/Services/PinnedDeckService.cs` (web implementation)

**Files to change:**
- `src/App/Ui.Shared/Layout/MainLayout.razor`
- `src/App/Ui.Shared/Layout/AppSidebar.razor`

**Changes:**
Define the interface in the Application layer:

```csharp
// src/App/Application/Features/PinnedDecks/IGetPinnedDecks.cs
namespace Mnemi.Application.Features.PinnedDecks;

public interface IGetPinnedDecks
{
    Task<IReadOnlyList<PinnedDeckItem>> ExecuteAsync(CancellationToken ct = default);
}
```

Then in `MainLayout.razor`, replace the hardcoded method:

```razor
@inject Mnemi.Application.Features.PinnedDecks.IGetPinnedDecks GetPinnedDecks

private IReadOnlyList<PinnedDeckItem> _pinnedDecks = Array.Empty<PinnedDeckItem>();

protected override async Task OnInitializedAsync()
{
    // ... existing auth logic ...
    _pinnedDecks = await GetPinnedDecks.ExecuteAsync();
}
```

And pass the data to the sidebar:

```razor
<AppSidebar
    ActiveRoute="@GetActiveRoute()"
    PinnedDecks="@_pinnedDecks"
    OnDeckSelected="@HandleDeckSelected" />
```

**Verification:** The sidebar should render the same sample data (during development) or real data (when the service is wired up). The `GetSamplePinnedDecks` method should no longer exist in `MainLayout.razor`.

---

### Phase 4 Verification Set (Agent-Verifiable)

```
[ ] 2.1  Verify SidebarIcon.razor exists in the Layout/ folder.
[ ] 2.1  Read SidebarNavLink.razor. It should reference <SidebarIcon> instead of inline <svg>.
[ ] 2.1  SidebarNavLink.razor should be under 50 lines.
[ ] 5.2  Verify IGetPinnedDecks interface exists in the Application layer.
[ ] 5.2  Read MainLayout.razor. GetSamplePinnedDecks should not exist.
[ ] 5.2  MainLayout.razor should inject IGetPinnedDecks and call ExecuteAsync in OnInitializedAsync.
```

### Phase 4 Manual UI Verification (Human-Performed)

> Open the app in a browser and perform these checks.

| # | Check | Steps | Expected Result |
|---|-------|-------|-----------------|
| 2.1 | Icon rendering parity | Navigate to all sidebar pages (Home, Review, Decks). Compare rendered icons to pre-refactor screenshots. | All icons render identically. No visual regressions. SVG paths are correct. |
| 2.1 | SidebarNavLink file size | Read SidebarNavLink.razor and count lines. | File is under 50 lines. Inline SVG is removed. Icon rendering is delegated to SidebarIcon.razor. |
| 5.2 | Deck data loading | Open the sidebar and observe the pinned decks section. | Decks load asynchronously without flicker. No sample data visible during load. Loading state is smooth. |
| 5.2 | Service injection verification | Open DevTools and check the network tab during page load. | No hardcoded data in the DOM. Deck data comes from the injected service (API call or local storage). |

---

## Phase 5: Future Considerations (Out of Scope for This Plan)

These items are acknowledged but deferred to separate efforts:

| Item | Reason for Deferral |
|------|---------------------|
| **3.2 Focus trap in mobile sidebar** | Requires JavaScript interop for focus management. Best implemented as a reusable `FocusTrap.razor` component in a future phase. |
| **6.1 Virtualization for long deck lists** | Only needed when users have 20+ pinned decks. Defer until the feature is actually used. |
| **1.3 CSS scoping (full strategy)** | The `mnemi-` prefix (Phase 1) is a sufficient interim solution. Full CSS modules or shadow DOM scoping requires Blazor framework changes. |

### Phase 5 Manual UI Verification (Future — Deferred)

> These checks will be performed when the deferred items are addressed in future phases.

| # | Check | Steps | Expected Result |
|---|-------|-------|-----------------|
| 3.2 | Focus trap on mobile | Open sidebar on a phone (or phone viewport). Tab through all focusable elements. Try tabbing past the last element. | Focus cycles within the sidebar. No focus escapes to elements behind the sidebar. Closing the sidebar returns focus to the toggle button. |
| 6.1 | Virtualized deck list | Create 25+ pinned decks. Open the sidebar and observe rendering performance. | Only visible deck items are rendered in the DOM. Scrolling remains smooth (60fps). No layout thrashing from large DOM trees. |
| 1.3 | CSS scoping verification | Use DevTools to inspect sidebar element styles. Check for unintended style inheritance from parent pages. | All sidebar styles are scoped to the `mnemi-` prefix. No styles leak out to the parent page. No external styles leak into the sidebar. |

---

## Execution Order Summary

```
Phase 1 (Zero-Impact)
  ├── 1.4 Fix hardcoded fallback colors
  ├── 1.3 Add mnemi- CSS prefix
  └── 6.2 Replace max-height with grid-template-rows
       │
       ▼
Phase 2 (Low-Impact)
  ├── 1.1 Remove duplicate CSS from app.css
  ├── 1.2 Remove duplicate stylesheet link
  ├── 4.1 Add :focus-visible to toggle
  ├── 4.3 Add ARIA list roles
  ├── 3.3 Add prefers-reduced-motion
  └── 2.4 Add @bind-Expanded to SidebarSection
       │
       ▼
Phase 3 (Medium-Impact)
  ├── 2.2 Accept QuickLinks as parameter
  ├── 2.3 Extract GetInitials to shared utility
  ├── 3.1 Fix sidebar sticky → fixed positioning
  ├── 5.1 Respect user sidebar preference
  └── 4.2 Add keyboard activation to pinned decks
       │
       ▼
Phase 4 (High-Impact)
  ├── 2.1 Extract icons to SidebarIcon.razor
  └── 5.2 Inject pinned deck data via service
```

---

## Risk Assessment

| Phase | Risk Level | Rollback Complexity |
|-------|-----------|-------------------|
| Phase 1 | **Minimal** — purely cosmetic CSS changes | Trivial — revert CSS file |
| Phase 2 | **Low** — accessibility improvements, no behavior change | Low — revert CSS and Razor changes |
| Phase 3 | **Medium** — changes component parameters and positioning | Medium — may require layout regression testing |
| Phase 4 | **High** — introduces new services and component structure | High — requires service registration rollback |

---

## References

- `docs/users/ui-problems-research.md` — Full problem inventory with code references
- `docs/dev/ui-architecture-guidelines.md` — Shared UI rules, dependency rules, design rules
- `specs/007-sidebar-layout/` — Original sidebar feature spec and tasks