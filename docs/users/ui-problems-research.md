# UI Problems Research — Sidebar, CSS Architecture & Page Layout

> Critical feedback on the existing sidebar implementation, CSS architecture, and page layout against Blazor industry best practices.

---

## 1. CSS Architecture Issues

### 1.1 Duplicate CSS Definitions Across Files

**Problem:** The same CSS classes are defined in two different files with conflicting or incomplete definitions.

**Affected files:**
- `src/App/Ui.Shared/wwwroot/Styles/sidebar-layout.css` (lines 28–35) — authoritative definitions
- `src/App/Ui.Web/wwwroot/css/app.css` (lines 15–24) — incomplete re-declarations

**Code reference — `sidebar-layout.css`:**
```css
.layout-shell {
    display: grid;
    grid-template-columns: 16rem minmax(0, 1fr);
    min-height: 100vh;
}

.layout-content {
    min-height: 100vh;
    overflow-y: auto;
    grid-column: 2;
}
```

**Code reference — `app.css`:**
```css
.layout-shell {
    /* Defined in Ui.Shared wwwroot/Styles/sidebar-layout.css */
    min-height: 100vh;
}

.layout-content {
    /* Defined in Ui.Shared wwwroot/Styles/sidebar-layout.css */
    min-height: 100vh;
}
```

**Impact:** If the grid definition changes in one file but not the other, the layout breaks silently. This is a maintenance hazard that will cause confusion for future developers.

---

### 1.2 Duplicate Stylesheet Loading

**Problem:** The `sidebar-layout.css` stylesheet is linked in two separate locations, causing it to be injected twice on every page load.

**Affected files:**
- `src/App/Ui.Shared/Layout/MainLayout.razor` (line 8)
- `src/App/Ui.Web/Components/App.razor` (line 11)

**Code reference — `MainLayout.razor`:**
```razor
<link rel="stylesheet" href="_content/Ui.Shared/Styles/sidebar-layout.css" />
```

**Code reference — `App.razor`:**
```razor
<link rel="stylesheet" href="_content/Ui.Shared/Styles/sidebar-layout.css" />
```

**Impact:** Unnecessary bandwidth consumption, potential cascade conflicts, and slower initial page load.

---

### 1.3 No CSS Scoping Strategy

**Problem:** All CSS class names are flat with BEM-style naming but no isolation mechanism. In a Razor Class Library consumed by both Web and MAUI hosts, class name collisions with other libraries or host-specific styles are possible.

**Affected file:** `src/App/Ui.Shared/wwwroot/Styles/sidebar-layout.css`

**Examples of flat class names:**
- `.sidebar-nav-link`
- `.sidebar-section__header`
- `.sidebar-user__avatar`
- `.pinned-decks__list`

**Impact:** Risk of style collisions when the RCL is consumed by multiple hosts or when third-party stylesheets are introduced.

---

### 1.4 Hardcoded Fallback Colors in CSS

**Problem:** Multiple CSS rules use hardcoded fallback colors that don't match the design system tokens.

**Affected file:** `src/App/Ui.Shared/wwwroot/Styles/sidebar-layout.css`

**Code reference (line 40):**
```css
background: var(--sidebar-background, #1e1e1e);
```

**Code reference (line 78):**
```css
color: var(--muted-foreground, #b0b0b0);
```

**Impact:** If theme variables are not applied, the app renders with inconsistent colors that don't match the intended design system.

---

## 2. Component Design Issues

### 2.1 Inline SVG Icons in SidebarNavLink

**Problem:** The `SidebarNavLink.razor` component contains all SVG path data inline in a `@switch` statement, coupling the component to specific icon shapes and bloating the component with presentation data.

**Affected file:** `src/App/Ui.Shared/Layout/SidebarNavLink.razor` (lines 8–42)

**Code reference:**
```razor
<svg class="sidebar-nav-link__icon" ... aria-hidden="true">
    @switch (IconKind)
    {
        case "home":
            <path d="m3 9 9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z" />
            <polyline points="9 22 9 12 15 12 15 22" />
            break;
        case "review":
            <path d="M2 3h6a4 4 0 0 1 4 4v14a3 3 0 0 0-3-3H2z" />
            <path d="M22 3h-6a4 4 0 0 0-4 4v14a3 3 0 0 1 3-3h7z" />
            break;
        case "decks":
            <rect x="3" y="3" width="7" height="9" rx="1" />
            <rect x="14" y="3" width="7" height="5" rx="1" />
            <rect x="14" y="12" width="7" height="9" rx="1" />
            break;
        default:
            <circle cx="12" cy="12" r="10" />
            <line x1="12" y1="8" x2="12" y2="16" />
            <line x1="8" y1="12" x2="16" y2="12" />
            break;
    }
</svg>
```

**Impact:**
- Coupling the component to specific icon shapes makes icon changes require a full component re-render
- Bloats the component with presentation data it shouldn't own
- Prevents icon reuse across the app
- Makes unit testing harder (SVG paths are part of the component's render tree)

---

### 2.2 Hardcoded Navigation Links in SidebarQuickLinks

**Problem:** The `QuickLinks` list is a `static readonly` field hardcoded inside the component, violating the principle that components should receive data through parameters.

**Affected file:** `src/App/Ui.Shared/Layout/SidebarQuickLinks.razor` (lines 14–20)

**Code reference:**
```razor
@code {
    [Parameter]
    public string ActiveRoute { get; set; } = "/";

    private static readonly IReadOnlyList<Models.SidebarNavItem> QuickLinks = new[]
    {
        new Models.SidebarNavItem("home", "Home", "/", "home"),
        new Models.SidebarNavItem("review", "Review", "/review", "review"),
        new Models.SidebarNavItem("decks", "Decks", "/decks", "decks"),
    };
}
```

**Impact:**
- Makes the component non-reusable (cannot be configured with different links)
- Impossible to test in isolation (no way to inject mock data)
- Prevents dynamic link configuration (e.g., feature-flagged links, role-based links)
- Violates the dependency injection pattern used throughout the rest of the app

---

### 2.3 Duplicate GetInitials Logic

**Problem:** The `GetInitials` method is implemented identically in two separate components.

**Affected file — `SidebarUserSection.razor` (lines 16–24):**
```razor
private static string GetInitials(string name)
{
    if (string.IsNullOrWhiteSpace(name))
        return "?";

    var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    if (parts.Length >= 2)
        return $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant();

    return name[..Math.Min(2, name.Length)].ToUpperInvariant();
}
```

**Affected file — `SidebarPinnedDecks.razor` (lines 34–44):**
```razor
private static string GetInitials(string name)
{
    if (string.IsNullOrWhiteSpace(name))
        return "?";

    var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    if (parts.Length >= 2)
        return $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant();

    return name[..Math.Min(2, name.Length)].ToUpperInvariant();
}
```

**Impact:** DRY violation — if the initials logic needs to change (e.g., handle non-Latin names, add middle initials), both implementations must be updated in sync.

---

### 2.4 SidebarSection State Not Externally Controllable

**Problem:** The `IsExpanded` state in `SidebarSection.razor` is managed internally via a private field with no external binding support.

**Affected file:** `src/App/Ui.Shared/Layout/SidebarSection.razor` (lines 54–63)

**Code reference:**
```razor
private bool _isExpanded = true;
private bool IsExpanded
{
    get => _isExpanded;
    set
    {
        if (_isExpanded != value)
        {
            _isExpanded = value;
        }
    }
}
```

**Impact:** Parent components cannot control or sync the expansion state. This prevents use cases like:
- Opening one section while closing others (accordion behavior)
- Persisting expansion state across page navigations
- Testing the component with different initial states

---

## 3. Layout & Responsiveness Issues

### 3.1 Sidebar Scrolls with Page on Desktop

**Problem:** The sidebar uses `position: sticky` with `height: 100vh`, but the content area has `overflow-y: auto`. This means the sidebar scrolls with the page rather than being truly fixed in place.

**Affected file:** `src/App/Ui.Shared/wwwroot/Styles/sidebar-layout.css`

**Code reference (lines 36–44):**
```css
.app-sidebar {
    display: flex;
    flex-direction: column;
    height: 100vh;
    position: sticky;
    top: 0;
    ...
}
```

**Code reference (lines 28–35):**
```css
.layout-shell {
    display: grid;
    grid-template-columns: 16rem minmax(0, 1fr);
    min-height: 100vh;
}

.layout-content {
    min-height: 100vh;
    overflow-y: auto;
    grid-column: 2;
}
```

**Impact:** On long pages, the sidebar content (user section, quick links, pinned decks) scrolls out of view along with the main content. Users lose access to navigation controls when scrolling down.

---

### 3.2 No Focus Trap in Mobile Sidebar

**Problem:** When the sidebar is open on mobile (position: fixed, slide-in), there is no focus trap. Keyboard users can tab into elements behind the sidebar backdrop.

**Affected file:** `src/App/Ui.Shared/Layout/AppSidebar.razor`

**Code reference (lines 17–22):**
```razor
<div class="sidebar-backdrop @(_sidebarOpen ? "sidebar-backdrop--visible" : "")"
     data-testid="sidebar-backdrop"
     @onclick="CloseSidebar"
     aria-hidden="true">
</div>
```

**Impact:** Keyboard and screen reader users can interact with page content behind the sidebar when it's open, leading to confusing and inaccessible behavior.

---

### 3.3 No `prefers-reduced-motion` Support

**Problem:** The sidebar has multiple CSS transitions but no support for users who prefer reduced motion.

**Affected file:** `src/App/Ui.Shared/wwwroot/Styles/sidebar-layout.css`

**Code reference (line 43):**
```css
transition: width 0.25s ease, transform 0.25s ease;
```

**Code reference (line 138):**
```css
transition: max-height 0.25s ease, opacity 0.2s ease, margin 0.25s ease;
```

**Code reference (line 178):**
```css
transition: background 0.15s ease, color 0.15s ease;
```

**Impact:** Users with vestibular disorders who use `prefers-reduced-motion: reduce` will experience potentially disorienting animations. This is an accessibility compliance issue.

---

## 4. Accessibility Issues

### 4.1 No Visible Focus Styles on Sidebar Toggle

**Problem:** The sidebar toggle button has no visible focus styles defined, making it invisible to keyboard users who need to locate and activate it.

**Affected file:** `src/App/Ui.Shared/wwwroot/Styles/sidebar-layout.css`

**Code reference (lines 310–328):**
```css
.sidebar-toggle {
    display: none;
}

@media (max-width: 1023px) {
    .sidebar-toggle {
        display: flex;
        position: fixed;
        top: 0.75rem;
        left: 0.75rem;
        z-index: 110;
        width: 2.25rem;
        height: 2.25rem;
        align-items: center;
        justify-content: center;
        border-radius: 0.5rem;
        background: var(--sidebar-background, #1e1e1e);
        border: 1px solid var(--sidebar-border, rgba(255, 255, 255, 0.08));
        color: var(--sidebar-foreground, #fafafa);
        cursor: pointer;
        font-size: 1.25rem;
        line-height: 1;
        transition: background 0.15s ease;
    }
}
```

**Impact:** Keyboard users cannot visually identify the toggle button when it receives focus, making the sidebar inaccessible via keyboard on tablet and phone viewports.

---

### 4.2 Pinned Deck Items Lack Keyboard Activation

**Problem:** Pinned deck items use `@onclick` on `<li>` elements without `@onkeydown` handlers for Enter/Space activation.

**Affected file:** `src/App/Ui.Shared/Layout/SidebarPinnedDecks.razor` (lines 12–22)

**Code reference:**
```razor
<li class="pinned-decks__item"
    data-testid="pinned-deck-@deck.Id"
    title="@deck.Name"
    @onclick="() => OnDeckSelected.InvokeAsync(deck)"
    @onclick:prevent_default>
    <span class="pinned-decks__icon" aria-hidden="true">@GetInitials(deck.Name)</span>
    <span class="pinned-decks__name">@deck.Name</span>
</li>
```

**Impact:** Screen reader users navigating with keyboard cannot activate pinned deck items. This is a WCAG 2.1 Level A violation (Keyboard / 2.1.1).

---

### 4.3 Missing ARIA List Roles

**Problem:** The `SidebarQuickLinks` component doesn't set `role="list"` on the container or `role="listitem"` on the items.

**Affected file:** `src/App/Ui.Shared/Layout/SidebarQuickLinks.razor` (lines 3–9)

**Code reference:**
```razor
<div class="sidebar-quick-links" data-testid="sidebar-quick-links">
    @foreach (var item in QuickLinks)
    {
        <Ui.Shared.Components.Layout.SidebarNavLink ... />
    }
</div>
```

**Impact:** Screen readers do not announce the navigation links as a list, reducing navigability for assistive technology users.

---

## 5. State Management Issues

### 5.1 Jarring Auto-Open on Desktop View State Change

**Problem:** The sidebar auto-opens when `ViewState.Desktop` is detected, overriding user preference.

**Affected file:** `src/App/Ui.Shared/Layout/AppSidebar.razor` (lines 44–53)

**Code reference:**
```razor
private void HandleViewStateChanged(ViewState newState)
{
    _currentViewState = newState;
    if (newState == ViewState.Phone)
    {
        _sidebarOpen = false;
    }
    else if (newState == ViewState.Desktop)
    {
        _sidebarOpen = true;  // <-- Overrides user preference
    }
    InvokeAsync(StateHasChanged);
}
```

**Impact:** If a user manually closes the sidebar on desktop and then changes viewport (e.g., rotates device, resizes browser), the sidebar suddenly re-opens, which is jarring and undermines user control.

---

### 5.2 Sample Data Hardcoded in Layout Component

**Problem:** `GetSamplePinnedDecks()` returns hardcoded sample data directly in the layout component.

**Affected file:** `src/App/Ui.Shared/Layout/MainLayout.razor` (lines 78–86)

**Code reference:**
```razor
private static IReadOnlyList<PinnedDeckItem> GetSamplePinnedDecks()
{
    return new[]
    {
        new PinnedDeckItem("1", "Spanish Verbs", "/review/deck1"),
        new PinnedDeckItem("2", "World Capitals", "/review/deck2"),
        new PinnedDeckItem("3", "Periodic Table", "/review/deck3"),
    };
}
```

**Impact:**
- The layout component owns data it should only present (violates separation of concerns)
- No way to inject real data during development or testing
- Blocks the path to real data integration (OAuth, file sync, etc.)

---

## 6. Performance Considerations

### 6.1 No Virtualization for Long Deck Lists

**Problem:** If a user has 50+ pinned decks, the `SidebarPinnedDecks` component will render all of them in the DOM.

**Affected file:** `src/App/Ui.Shared/Layout/SidebarPinnedDecks.razor` (lines 10–24)

**Code reference:**
```razor
<ul class="pinned-decks__list" data-testid="pinned-decks-list">
    @foreach (var deck in PinnedDecks)
    {
        <li class="pinned-decks__item" ...>
            ...
        </li>
    }
</ul>
```

**Impact:** Rendering hundreds of DOM nodes for long deck lists degrades scroll performance and increases memory usage. Blazor's `Virtualize<T>` component should be used for lists that could grow beyond ~20 items.

---

### 6.2 `max-height` Animation Triggers Layout Recalculation

**Problem:** The collapsible section uses `max-height` for animation, which requires knowing the content height and triggers expensive layout recalculation on every toggle.

**Affected file:** `src/App/Ui.Shared/wwwroot/Styles/sidebar-layout.css` (lines 136–143)

**Code reference:**
```css
.sidebar-section__content {
    overflow: hidden;
    max-height: 0;
    opacity: 0;
    transition: max-height 0.25s ease, opacity 0.2s ease, margin 0.25s ease;
    margin-top: 0;
}

.sidebar-section__content--open {
    max-height: 50rem; /* large enough for any section, scrolls if needed */
    opacity: 1;
    margin-top: 0.25rem;
}
```

**Impact:** The `max-height: 50rem` value is a guess — if content exceeds it, it clips. If it's much larger than needed, the animation is unnecessarily slow. The `max-height` approach triggers full layout recalculation on every toggle, whereas `grid-template-rows` transition (`0fr` to `1fr`) is GPU-accelerated and doesn't require knowing the content height.

---

## Summary of Priority Fixes

| Priority | Issue | File(s) | Impact |
|----------|-------|---------|--------|
| **High** | Duplicate CSS definitions across files | `sidebar-layout.css`, `app.css` | Maintenance risk, potential layout bugs |
| **High** | Duplicate stylesheet loading | `MainLayout.razor`, `App.razor` | Performance, cascade conflicts |
| **High** | Keyboard inaccessibility on pinned decks | `SidebarPinnedDecks.razor` | WCAG 2.1 Level A violation |
| **Medium** | Hardcoded nav links in component | `SidebarQuickLinks.razor` | Testability, reusability |
| **Medium** | Duplicate `GetInitials` logic | `SidebarUserSection.razor`, `SidebarPinnedDecks.razor` | DRY violation |
| **Medium** | No `prefers-reduced-motion` support | `sidebar-layout.css` | Accessibility compliance |
| **Medium** | Sidebar scrolls with page on desktop | `sidebar-layout.css` | UX — nav controls scroll out of view |
| **Low** | Inline SVG icons in NavLink | `SidebarNavLink.razor` | Maintainability |
| **Low** | No virtualization for long lists | `SidebarPinnedDecks.razor` | Performance at scale |
| **Low** | `max-height` animation performance | `sidebar-layout.css` | Layout recalculation on toggle |
| **Low** | No focus trap in mobile sidebar | `AppSidebar.razor` | Accessibility |
| **Low** | Jarring auto-open on viewport change | `AppSidebar.razor` | UX — overrides user preference |
| **Low** | Sample data in layout component | `MainLayout.razor` | Separation of concerns |
| **Low** | No CSS scoping strategy | `sidebar-layout.css` | Collision risk in RCL |
| **Low** | Hardcoded fallback colors | `sidebar-layout.css` | Inconsistent rendering |
| **Low** | Section state not externally controllable | `SidebarSection.razor` | Limited reusability |

---

## References

- **WCAG 2.1 Guidelines:** [2.1.1 Keyboard](https://www.w3.org/TR/WCAG21/#keyboard), [2.3.3 Animation from Interactions](https://www.w3.org/TR/WCAG21/#animation-from-interactions)
- **Blazor Best Practices:** [Microsoft Docs — Blazor rendering](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/render-modes)
- **CSS Performance:** [CSS Transitions — `max-height` vs `grid-template-rows`](https://developer.mozilla.org/en-US/docs/Web/CSS/transition)
- **Project Guidelines:** `docs/dev/ui-architecture-guidelines.md` — Shared UI rules, dependency rules, design rules