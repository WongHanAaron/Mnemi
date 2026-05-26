# CSS Migration Plan — Component-Scoped CSS

## Goal

Migrate all component-specific CSS from global stylesheets to Blazor scoped CSS (`.razor.css`) files, while preserving a minimal global stylesheet for design tokens, resets, and platform-level styles.

## Guiding Principles

- **Least impact first**: Each phase is independently verifiable and low-risk
- **One component family per phase**: Group related components to minimize verification overhead
- **Manual UI verification after each phase**: Visual regression is the primary quality gate
- **Preserve BEM naming**: Keep the existing naming convention within scoped files for readability
- **Design tokens stay global**: `:root` custom properties remain in a single global file

---

## Phase 0 — Foundation & Preparation (Lowest Impact)

**Goal:** Consolidate global CSS, fix duplicates, and prepare the groundwork. No component changes yet.

### Tasks

1. **Create `Ui.Shared/wwwroot/Styles/tokens.css`**
   - Extract all `:root` custom properties from `home-blueprint-theme.css`
   - This becomes the single source of truth for design tokens

2. **Create `Ui.Shared/wwwroot/Styles/reset.css`**
   - Consolidate all `body`, `html`, and `*` rules from:
     - `Ui.Web/wwwroot/css/app.css` (html, body rules)
     - `home-blueprint-theme.css` (body rule)
     - `sidebar-layout.css` (body rule, prefers-reduced-motion)
   - Single file for global resets and base typography

3. **Consolidate `#blazor-error-ui`**
   - Remove duplicate from `Ui.Web/wwwroot/css/app.css`
   - Keep the version in `Ui.Shared/wwwroot/app.css` (more complete)

4. **Update `App.razor`**
   - Replace individual stylesheet references with the new consolidated files:
     ```html
     <link rel="stylesheet" href="_content/Ui.Shared/lib/bootstrap/dist/css/bootstrap.min.css" />
     <link rel="stylesheet" href="_content/Ui.Shared/Styles/tokens.css" />
     <link rel="stylesheet" href="_content/Ui.Shared/Styles/reset.css" />
     <link rel="stylesheet" href="_content/Ui.Shared/app.css" />
     <link rel="stylesheet" href="css/app.css" />
     <link rel="stylesheet" href="Ui.Web.styles.css" />
     ```
   - Remove `home-blueprint-theme.css` reference (will be replaced as components migrate)

5. **Move `sidebar-layout.css` load from `MainLayout.razor` to `App.razor`**
   - Remove `<link>` from `MainLayout.razor`
   - Add to `App.razor` head (temporary, until sidebar components migrate)

### Verification Checklist

- [ ] Web app loads without console errors
- [ ] Sidebar renders correctly on desktop (≥1024px)
- [ ] Sidebar renders correctly on tablet (768–1023px)
- [ ] Sidebar renders correctly on phone (≤767px)
- [ ] Home dashboard renders with correct colors and layout
- [ ] Auth pages (login/signup) render correctly
- [ ] No visual regression in any view
- [ ] Build succeeds with no warnings

---

## Phase 1 — Sidebar Components (Low Impact)

**Goal:** Migrate all sidebar-related CSS to component-scoped files.

**Rationale:** Sidebar components are well-isolated with clear BEM prefixes (`sidebar-*`, `app-sidebar-*`, `pinned-decks-*`). Low risk of cross-component leakage.

### Tasks

1. **`AppSidebar.razor.css`**
   - Move from `sidebar-layout.css`:
     - `.app-sidebar`, `.app-sidebar--open`, `.app-sidebar__header`, `.app-sidebar__content`
     - `.sidebar-toggle`, `.sidebar-toggle--active`, `.sidebar-toggle__icon`, `.sidebar-toggle__icon--active`
     - `.sidebar-backdrop`, `.sidebar-backdrop--visible`
     - All `@media` rules targeting these classes

2. **`SidebarUserSection.razor.css`**
   - Move from `sidebar-layout.css`:
     - `.sidebar-user`, `.sidebar-user__avatar`, `.sidebar-user__info`, `.sidebar-user__name`

3. **`SidebarNavLink.razor.css`**
   - Move from `sidebar-layout.css`:
     - `.sidebar-nav-link`, `.sidebar-nav-link--active`, `.sidebar-nav-link__icon`

4. **`SidebarSection.razor.css`**
   - Move from `sidebar-layout.css`:
     - `.sidebar-section`, `.sidebar-section__header`, `.sidebar-section__chevron`, `.sidebar-section__chevron--open`
     - `.sidebar-section__pinicon`, `.sidebar-section__pinicon--open`
     - `.sidebar-section__content`, `.sidebar-section__content--open`

5. **`SidebarQuickLinks.razor.css`**
   - Move from `sidebar-layout.css`:
     - `.sidebar-quick-links`, `.sidebar-quick-links ul`, `.sidebar-quick-links li`

6. **`SidebarPinnedDecks.razor.css`**
   - Move from `sidebar-layout.css`:
     - `.pinned-decks__header`, `.pinned-decks__pin-icon`, `.pinned-decks__pin-icon--open`
     - `.pinned-decks__list`, `.pinned-decks__item`, `.pinned-decks__icon`, `.pinned-decks__name`, `.pinned-decks__empty`

7. **`SidebarIcon.razor.css`**
   - No CSS to migrate (component passes `Class` param)
   - Create empty file for consistency if desired

8. **Clean up `sidebar-layout.css`**
   - After all moves, only `.layout-shell` and `.layout-content` remain
   - Move these to `MainLayout.razor.css` (Phase 2)
   - Delete `sidebar-layout.css`

9. **Update `App.razor`**
   - Remove `sidebar-layout.css` reference

### Verification Checklist

- [ ] Sidebar renders correctly on desktop (≥1024px) — full width, all sections visible
- [ ] Sidebar toggle button works on tablet/phone
- [ ] Sidebar slides in/out on phone (≤767px) with backdrop
- [ ] Sidebar collapses to icons-only on tablet (768–1023px)
- [ ] Sidebar expands on tap on tablet
- [ ] Quick links highlight active route correctly
- [ ] Pinned decks section renders with correct icons and names
- [ ] Collapsible sections expand/collapse with animation
- [ ] User avatar and name display correctly
- [ ] No console errors or missing style warnings

---

## Phase 2 — Layout Shell (Low Impact)

**Goal:** Migrate shell-level layout CSS and auth layout.

### Tasks

1. **`MainLayout.razor.css`**
   - Move remaining shell rules:
     - `.layout-shell`, `.layout-content`
   - Move auth guard rules from `Ui.Web/wwwroot/css/app.css`:
     - `.auth-guard-loading`, `.auth-guard-redirect`

2. **`AuthLayout.razor.css`**
   - Move from `auth.css`:
     - `.auth-layout` and all `::before`/`::after` pseudo-element rules

3. **Clean up `Ui.Web/wwwroot/css/app.css`**
   - After removing auth guard rules, only `#blazor-error-ui` remains (already in Shared)
   - Delete file if empty, or keep for web-specific overrides

### Verification Checklist

- [ ] Main layout grid renders correctly (sidebar + content)
- [ ] Auth guard loading message displays during auth check
- [ ] Auth guard redirect message displays before navigation
- [ ] Auth layout (login page) has correct background and centering
- [ ] Dotted radial gradient background on auth pages renders correctly
- [ ] No visual regression in page structure

---

## Phase 3 — Home Dashboard Components (Medium Impact)

**Goal:** Migrate all home dashboard CSS to component-scoped files.

**Rationale:** Home components have a large CSS surface area with many BEM classes. Migrating them together ensures consistency.

### Tasks

1. **`HomeDashboard.razor.css`**
   - Move from `home-blueprint-theme.css`:
     - `.home-shell`, `.home-shell--adaptive`
     - `.home-main`
     - `.home-welcome`
     - `.home-eyebrow`, `.home-title`, `.home-subtitle`
     - `.home-primary-action`
     - `.home-section`, `.home-section__header`, `.home-section__empty`
     - `.home-state-message`
     - `.home-quick-stats-grid`
     - `.home-deck-grid`
     - `@media (max-width: 720px)` rules for home components

2. **`QuickStatTile.razor.css`**
   - Move from `home-blueprint-theme.css`:
     - `.home-quick-stat`, `.home-quick-stat--up`, `.home-quick-stat--down`, `.home-quick-stat--flat`
     - `.home-quick-stat__label`, `.home-quick-stat__value`, `.home-quick-stat__trend`

3. **`DeckCard.razor.css`**
   - Move from `home-blueprint-theme.css`:
     - `.home-deck-card`, `.home-deck-card:hover`
     - `.home-deck-card__cover`, `.home-deck-card__cover-fallback`, `.home-deck-card__cover-token`
     - `.home-deck-card__content`, `.home-deck-card__title`, `.home-deck-card__subtitle`
     - `.home-deck-card__meta`, `.home-deck-card__status`, `.home-deck-card__progress`
     - `.home-deck-card__action`
     - `.home-deck-card__badge` (if still used)
     - `.home-deck-card__button` (if still used)
     - `.home-deck-card__footer`, `.home-deck-card__description`

4. **`HomeSideNav.razor.css`** (if still in use)
   - Move from `home-blueprint-theme.css`:
     - `.home-side-nav`, `.home-side-nav__list`, `.home-side-nav__item`, `.home-side-nav__item--active`

5. **Delete `home-blueprint-theme.css`**
   - After all rules are migrated, the file should be empty
   - Remove from `App.razor` if not already done

### Verification Checklist

- [ ] Home dashboard renders with correct claymorphism styling
- [ ] Welcome section shows greeting and primary action button
- [ ] Quick stats grid shows 3 columns on desktop, 1 column on phone
- [ ] Deck cards render with correct layout, badges, and buttons
- [ ] Hover effects work on stat tiles and deck cards
- [ ] Primary action button has gradient and hover animation
- [ ] Responsive layout at 720px breakpoint works correctly
- [ ] Empty/loading/missing data states render correctly
- [ ] No visual regression in home view

---

## Phase 4 — Auth Components (Medium Impact)

**Goal:** Migrate auth page CSS and fix orphaned component styles.

### Tasks

1. **`LoginButton.razor.css`**
   - Create scoped CSS for classes currently in Razor but **not yet defined**:
     - `.auth-login-panel`, `.auth-login-card`
     - `.auth-login-header`, `.auth-login-title`, `.auth-login-subtitle`
     - `.auth-login-providers`
     - `.auth-provider-btn`, `.auth-provider-btn--google`, `.auth-provider-btn--github`
     - `.auth-provider-icon`, `.auth-provider-label`
     - `.auth-login-error`
   - Reuse existing `.auth-provider-btn` rules from `auth.css` as base

2. **`UserMenu.razor.css`**
   - Create scoped CSS for classes currently in Razor but **not yet defined**:
     - `.auth-user-menu`, `.auth-user-menu__trigger`
     - `.auth-user-menu__avatar`, `.auth-user-menu__avatar-fallback`
     - `.auth-user-menu__name`
     - `.auth-user-menu__chevron`, `.auth-user-menu__chevron--open`
     - `.auth-user-menu__dropdown`, `.auth-user-menu__dropdown-header`
     - `.auth-user-menu__dropdown-email`
     - `.auth-user-menu__dropdown-actions`, `.auth-user-menu__dropdown-btn`
     - `.auth-user-menu__dropdown-btn--danger`

3. **Migrate remaining `auth.css` rules**
   - `.auth-card`, `.auth-card__header`, `.auth-card__icon`, `.auth-card__title`, `.auth-card__description`
   - `.auth-card__content`, `.auth-card__footer`, `.auth-card__footer-text`, `.auth-card__footer-link`
   - `.auth-providers`
   - `.auth-provider-btn` (shared base, may need to be in a shared component or duplicated)
   - `.auth-divider`, `.auth-divider__text`
   - `.auth-form`, `.auth-form__field`, `.auth-form__label`, `.auth-form__label-row`
   - `.auth-form__forgot-link`, `.auth-form__input`
   - `.auth-submit-btn`
   - `.auth-terms`, `.auth-terms__link`
   - `.auth-error`
   - `.auth-card__spinner`, `@keyframes auth-spin`
   - Determine which Razor component owns each rule and move accordingly
   - If no component exists yet (e.g., for a future login form page), create a placeholder component or keep in a temporary `auth-shared.razor.css`

4. **Delete `auth.css`**
   - After all rules are migrated

### Verification Checklist

- [ ] Login page renders with correct card layout and background
- [ ] OAuth provider buttons (Google, GitHub) render correctly
- [ ] Login button hover and disabled states work
- [ ] Error message displays correctly on failed login
- [ ] User menu dropdown opens/closes in sidebar
- [ ] User avatar and name display correctly
- [ ] Sign out button renders with danger styling
- [ ] No visual regression in auth flow

---

## Phase 5 — Platform-Specific Cleanup (Highest Impact)

**Goal:** Final cleanup of remaining global CSS and platform-specific concerns.

### Tasks

1. **`MainLayout.razor.css`** (update)
   - Verify `.layout-shell` and `.layout-content` are correctly scoped
   - Add any platform-specific overrides

2. **Create `WebViewStateService`** (if not already done)
   - Replace `StubViewStateService` in `Ui.Web/Program.cs`
   - Register in DI

3. **Audit `Ui.Shared/wwwroot/app.css`**
   - Keep: `.valid`, `.invalid`, `.validation-message` (Blazor validation — global)
   - Keep: `.blazor-error-boundary` (Blazor error UI — global)
   - Keep: `#blazor-error-ui` (Blazor error UI — global)
   - Keep: `.form-floating` overrides (Bootstrap override — global)
   - Remove anything else that can be scoped

4. **Audit `Ui/wwwroot/app.css` (MAUI)**
   - Keep: `.status-bar-safe-area` and `@supports` block (platform-specific — global)

5. **Final audit of all `.razor.css` files**
   - Verify Blazor's scoped CSS hash classes are being generated
   - Check `Ui.Web.styles.css` for correct scoped class output
   - Ensure no duplicate rules between scoped and global files

6. **Update all project references**
   - Verify `Ui.Web.styles.css` includes all scoped CSS
   - Clean and rebuild solution

### Verification Checklist

- [ ] Full regression test of all pages
- [ ] Web app builds and runs without errors
- [ ] MAUI app builds without errors (if build environment available)
- [ ] All design tokens are resolved correctly
- [ ] No duplicate CSS rules in final bundle
- [ ] `Ui.Web.styles.css` contains scoped classes for all migrated components
- [ ] Browser DevTools shows scoped class attributes on elements
- [ ] No console warnings about unused CSS

---

## Component File Map (After Migration)

```
Ui.Shared/
├── wwwroot/
│   ├── app.css                          ← Blazor system classes only
│   └── Styles/
│       ├── tokens.css                   ← :root custom properties
│       └── reset.css                    ← html/body resets, reduced-motion
│
├── Layout/
│   ├── MainLayout.razor
│   ├── MainLayout.razor.css             ← .layout-shell, .layout-content, auth guards
│   ├── AuthLayout.razor
│   ├── AuthLayout.razor.css             ← .auth-layout
│   ├── AppSidebar.razor
│   ├── AppSidebar.razor.css             ← .app-sidebar, .sidebar-toggle, .sidebar-backdrop
│   ├── SidebarUserSection.razor
│   ├── SidebarUserSection.razor.css     ← .sidebar-user
│   ├── SidebarNavLink.razor
│   ├── SidebarNavLink.razor.css         ← .sidebar-nav-link, .sidebar-nav-link--active
│   ├── SidebarSection.razor
│   ├── SidebarSection.razor.css         ← .sidebar-section, .sidebar-section__header
│   ├── SidebarQuickLinks.razor
│   ├── SidebarQuickLinks.razor.css      ← .sidebar-quick-links, .sidebar-quick-links__item
│   ├── SidebarPinnedDecks.razor
│   ├── SidebarPinnedDecks.razor.css     ← .sidebar-pinned-decks, .sidebar-pinned-deck
│   ├── SidebarIcon.razor
│   ├── SidebarIcon.razor.css            ← .sidebar-icon
│
│   ├── HomeDashboard.razor
│   ├── HomeDashboard.razor.css          ← .home-dashboard, .home-header, .home-content
│   ├── QuickStatTile.razor
│   ├── QuickStatTile.razor.css          ← .home-quick-stat, .home-quick-stat__value
│   ├── DeckCard.razor
│   ├── DeckCard.razor.css               ← .home-deck-card, .home-deck-card__title
│   ├── HomeSideNav.razor
│   ├── HomeSideNav.razor.css            ← .home-side-nav
│
│   └── Auth/
│       ├── LoginButton.razor
│       ├── LoginButton.razor.css        ← .auth-login-panel, .auth-login-card, .auth-provider-btn
│       ├── UserMenu.razor
│       └── UserMenu.razor.css           ← .auth-user-menu
│
│   ├── ReconnectModal.razor             ← (already scoped, no change needed)
│   └── ReconnectModal.razor.css
```

---

## Files Deleted After Migration

| File | Reason |
|------|--------|
| `home-blueprint-theme.css` | All component-specific rules migrated; only `:root` tokens moved to `tokens.css` |
| `sidebar-layout.css` | All component-specific rules migrated to `*.razor.css` files |
| `auth.css` | All component-specific rules migrated to `*.razor.css` files |
| `Ui.Web/wwwroot/css/app.css` | Consolidated into `Ui.Shared/app.css` + `Styles/reset.css` |
| `Ui.Web.Client/wwwroot/css/app.css` | Consolidated into `Ui.Shared/app.css` + `Styles/reset.css` |
| `Ui.Maui/wwwroot/css/app.css` | Consolidated into `Ui.Shared/app.css` + `Styles/reset.css` |

---

## Files Retained (Global)

| File | Purpose |
|------|---------|
| `Ui.Shared/wwwroot/Styles/tokens.css` | Design tokens (`:root` custom properties) — shared across all components |
| `Ui.Shared/wwwroot/Styles/reset.css` | Global resets (html/body/base typography) — shared across all platforms |
| `Ui.Shared/wwwroot/app.css` | Blazor system classes (`#blazor-error-ui`, focus styles) — platform-level |
| `Ui.Web/wwwroot/css/app.css` | Platform-specific overrides only (if any remain after consolidation) |
| `Ui.Maui/wwwroot/css/app.css` | Platform-specific overrides only (if any remain after consolidation) |
| `Ui.Shared/lib/bootstrap/dist/css/bootstrap.min.css` | Third-party framework styles (unchanged) |

---

## Risk Assessment

| Phase | Risk Level | Mitigation |
|-------|-----------|------------|
| Phase 0 | **Low** | Pure consolidation; no component changes. Visual regression unlikely. |
| Phase 1 | **Low** | Sidebar is self-contained. Isolated verification scope. |
| Phase 2 | **Low** | Layout shell is minimal. Easy to rollback if needed. |
| Phase 3 | **Medium** | Home dashboard has more visual complexity. Requires careful visual verification. |
| Phase 4 | **Medium** | Auth components may have platform-specific rendering. Orphaned CSS fix is a bonus. |
| Phase 5 | **Low** | Cleanup only. No functional changes. |

---

## Estimated Effort

| Phase | Estimated Time | Complexity |
|-------|---------------|------------|
| Phase 0 | 1–2 hours | Low |
| Phase 1 | 2–3 hours | Low |
| Phase 2 | 1–2 hours | Low |
| Phase 3 | 3–4 hours | Medium |
| Phase 4 | 2–3 hours | Medium |
| Phase 5 | 1 hour | Low |
| **Total** | **10–15 hours** | **Medium** |

---

## Dependencies Between Phases

```
Phase 0 (Foundation)
    │
    ├──→ Phase 1 (Sidebar) ──→ Phase 2 (Layout)
    │                              │
    │                              └──→ Phase 3 (Home Dashboard)
    │
    └──→ Phase 4 (Auth) ──→ Phase 5 (Cleanup)
```

**Key dependencies:**
- Phase 0 must complete first (tokens.css, reset.css are prerequisites)
- Phase 1 and Phase 4 are independent (can be done in parallel)
- Phase 2 depends on Phase 1 (MainLayout loads sidebar components)
- Phase 3 depends on Phase 2 (HomeDashboard uses layout shell)
- Phase 5 is final cleanup (depends on all migration phases)

---

## Manual UI Verification Checklist (All Phases)

After each phase completes, perform these visual checks:

### Layout & Structure
- [ ] Sidebar collapses and expands correctly
- [ ] Sidebar backdrop appears on mobile when sidebar is open
- [ ] Main content area fills available space
- [ ] Auth pages render correctly (if applicable)
- [ ] Home dashboard displays all sections

### Typography & Colors
- [ ] All text uses correct font family and size
- [ ] Color scheme matches design (dark theme)
- [ ] Hover states have correct colors
- [ ] Active/selected states are visually distinct

### Responsive Behavior
- [ ] Desktop view: sidebar always visible
- [ ] Tablet view: sidebar collapses to icons
- [ ] Mobile view: sidebar is hidden, toggle button works
- [ ] No horizontal scroll at any breakpoint

### Interactive Elements
- [ ] Sidebar navigation links highlight correctly
- [ ] Deck cards display hover effects
- [ ] Quick stat tiles render with correct icons
- [ ] Login/signup forms are functional
- [ ] User menu dropdown opens/closes

### Platform-Specific
- [ ] Web (Server): Components render server-side correctly
- [ ] Web (WASM): Components render client-side correctly
- [ ] MAUI: Components render in WebView correctly
- [ ] ReconnectModal appears/disappears correctly

---

## Notes

- **Blazor scoped CSS** uses the `.razor.css` convention. The framework automatically generates unique class names in production builds.
- **BEM naming** is preserved within scoped files for readability, but the BEM prefix becomes the scoped class name (e.g., `.sidebar-nav-link` becomes a scoped class).
- **Design tokens** (`:root` custom properties) must remain global because they need to be accessible from all components.
- **Bootstrap** is not migrated — it's a third-party dependency and should remain as-is.
- **Platform-specific overrides** (if any) should be kept in each platform's `app.css` after consolidation.
│   ├── SidebarNavLink.razor
│   ├── SidebarNavLink.razor.css         ← .sidebar-nav-link
│   ├── SidebarSection.razor
│   ├── SidebarSection.razor.css         ← .sidebar-section
│   ├── SidebarQuickLinks.razor
│   ├── SidebarQuickLinks.razor.css      ← .sidebar-quick-links
│   ├── SidebarPinnedDecks.razor
│   ├── SidebarPinnedDecks.razor.css     ← .pinned-decks
│   └── SidebarIcon.razor                ← (no CSS, passes Class param)
│
├── Components/
│   ├── Home/
│   │   ├── HomeDashboard.razor
│   │   ├── HomeDashboard.razor.css      ← .home-shell, .home-main, .home-welcome, .home-section, grids
│   │   ├── QuickStatTile.razor
│   │   ├── QuickStatTile.razor.css      ← .home-quick-stat
│   │   ├── DeckCard.razor
│   │   ├── DeckCard.razor.css           ← .home-deck-card
│   │   ├── HomeSideNav.razor
│   │   └── HomeSideNav.razor.css       ← .home-side-nav
│   └── Auth/
│       ├── LoginButton.razor
│       ├── LoginButton.razor.css        ← .auth-login-panel, .auth-login-card, .auth-provider-btn
│       ├── UserMenu.razor
│       └── UserMenu.razor.css           ← .auth-user-menu
│
├── ReconnectModal.razor                 ← (already scoped, no change needed)
└── ReconnectModal.razor.css
```

## Files Deleted After Migration

| File | Reason |
|------|--------|
| `home-blueprint-theme.css` | All rules migrated to component `.razor.css` files |
| `sidebar-layout.css` | All rules migrated to component `.razor.css` files |
| `auth.css` | All rules migrated to component `.razor.css` files |
| `Ui.Web/wwwroot/css/app.css` | Rules consolidated into Shared or deleted |

## Files Retained (Global)

| File | Purpose |
|------|---------|
| `Ui.Shared/wwwroot/app.css` | Blazor system classes (validation, error UI, form-floating) |
| `Ui.Shared/wwwroot/Styles/tokens.css` | Design tokens (`:root` custom properties) |
| `Ui.Shared/wwwroot/Styles/reset.css` | Global resets (html, body, `*`, reduced-motion) |
| `Ui/wwwroot/app.css` | MAUI platform-specific styles (status-bar-safe-area) |

## Risk Assessment

| Phase | Risk | Rollback Strategy |
|-------|------|-------------------|
| 0 | **Low** — Only file reorganization, no style changes | Revert `App.razor` to original `<link>` tags |
| 1 | **Low** — Sidebar is isolated from other components | Restore `sidebar-layout.css` and revert `App.razor` |
| 2 | **Low** — Layout shell is simple, few rules | Restore original CSS files |
| 3 | **Medium** — Large surface area, many rules | Restore `home-blueprint-theme.css` |
| 4 | **Medium** — Auth flow is critical path | Restore `auth.css` |
| 5 | **High** — Platform-specific, affects all platforms | Full rollback to pre-migration state |

## Estimated Effort

| Phase | Estimated Time | Complexity |
|-------|---------------|------------|
| 0 | 1–2 hours | Low |
| 1 | 2–3 hours | Low |
| 2 | 1 hour | Low |
| 3 | 3–4 hours | Medium |
| 4 | 2–3 hours | Medium |
| 5 | 2–3 hours | High |
| **Total** | **11–15 hours** | |

## Dependencies Between Phases

```
Phase 0 (Foundation)
    ↓
Phase 1 (Sidebar Components)
    ↓
Phase 2 (Layout Shell)
    ↓
Phase 3 (Home Dashboard)
    ↓
Phase 4 (Auth Components)
    ↓
Phase 5 (Platform Cleanup)
```

Each phase depends on the previous phase being complete and verified. Do not skip phases.