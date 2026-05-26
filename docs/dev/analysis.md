# CSS Architecture Analysis — Mnemi UI

## Executive Summary

The Mnemi project uses a **global CSS architecture** with zero component-scoped styles. All styling is delivered through globally-loaded stylesheets with BEM-like class naming conventions. While the naming is well-organized, the approach lacks CSS encapsulation, creating potential for style collisions and making it difficult to reason about component ownership of styles.

---

## Current CSS File Inventory

| # | File | Location | Scope | Size (lines) | Purpose |
|---|------|----------|-------|--------------|---------|
| 1 | `home-blueprint-theme.css` | `Ui.Shared/wwwroot/Styles/` | **Global** | ~260 | Design tokens (`:root`), home dashboard layout, claymorphism theme |
| 2 | `sidebar-layout.css` | `Ui.Shared/wwwroot/Styles/` | **Global** | ~340 | Sidebar shell, navigation, responsive breakpoints, backdrop |
| 3 | `auth.css` | `Ui.Web/wwwroot/css/` | **Global** | ~280 | Login/signup page card, OAuth buttons, form fields |
| 4 | `app.css` (Ui.Web) | `Ui.Web/wwwroot/css/` | **Global** | ~45 | Auth guard loading states, Blazor error UI |
| 5 | `app.css` (Ui.Shared) | `Ui.Shared/wwwroot/` | **Global** | ~40 | Blazor validation, error boundary, form floating labels |
| 6 | `app.css` (Ui) | `Ui/wwwroot/` | **Global** | ~20 | iOS safe-area insets |
| 7 | `ReconnectModal.razor.css` | `Ui.Web.Client/Layout/` | **Scoped** | ~130 | Blazor SignalR reconnect dialog (only scoped CSS file) |

**Total: 7 CSS files, 6 global, 1 scoped.**

---

## CSS Loading Chain

### Web App (`App.razor`)
```html
<!-- Load order in App.razor <head> -->
1. _content/Ui.Shared/lib/bootstrap/dist/css/bootstrap.min.css   (Bootstrap 5)
2. _content/Ui.Shared/app.css                                    (validation, error UI)
3. _content/Ui.Shared/Styles/home-blueprint-theme.css            (design tokens + home)
4. css/app.css                                                   (auth guard, error UI)
5. Ui.Web.styles.css                                             (Blazor scoped CSS aggregator)
```

### MainLayout.razor
```html
<!-- Loaded inline at component level -->
<link rel="stylesheet" href="_content/Ui.Shared/Styles/sidebar-layout.css" />
```

**Problem:** `sidebar-layout.css` is loaded inside `MainLayout.razor` rather than in `App.razor`, creating a flash-of-unstyled-content risk and inconsistent loading strategy.

---

## Component Inventory & CSS Class Usage

### Layout Components (Ui.Shared/Layout/)

| Component | CSS Classes Used | Source File |
|-----------|-----------------|-------------|
| `MainLayout.razor` | `auth-guard-loading`, `auth-guard-redirect`, `layout-shell`, `layout-content` | `app.css`, `sidebar-layout.css` |
| `AppSidebar.razor` | `sidebar-toggle`, `sidebar-toggle--active`, `sidebar-toggle__icon`, `sidebar-backdrop`, `sidebar-backdrop--visible`, `app-sidebar`, `app-sidebar--open`, `app-sidebar__header`, `app-sidebar__content` | `sidebar-layout.css` |
| `SidebarUserSection.razor` | `sidebar-user`, `sidebar-user__avatar`, `sidebar-user__info`, `sidebar-user__name` | `sidebar-layout.css` |
| `SidebarNavLink.razor` | `sidebar-nav-link`, `sidebar-nav-link--active`, `sidebar-nav-link__icon` | `sidebar-layout.css` |
| `SidebarSection.razor` | `sidebar-section`, `sidebar-section__header`, `sidebar-section__chevron`, `sidebar-section__chevron--open`, `sidebar-section__pinicon`, `sidebar-section__pinicon--open`, `sidebar-section__content`, `sidebar-section__content--open` | `sidebar-layout.css` |
| `SidebarQuickLinks.razor` | `sidebar-quick-links` | `sidebar-layout.css` |
| `SidebarPinnedDecks.razor` | `pinned-decks__empty`, `pinned-decks__list`, `pinned-decks__item`, `pinned-decks__icon`, `pinned-decks__name` | `sidebar-layout.css` |
| `SidebarIcon.razor` | (passes `Class` param, no own classes) | — |
| `AuthLayout.razor` | `auth-layout` | `auth.css` |

### Home Components (Ui.Shared/Components/Home/)

| Component | CSS Classes Used | Source File |
|-----------|-----------------|-------------|
| `HomeDashboard.razor` | `home-shell`, `home-shell--adaptive`, `home-main`, `home-welcome`, `home-eyebrow`, `home-title`, `home-subtitle`, `home-primary-action`, `home-section`, `home-section__header`, `home-section__empty`, `home-state-message`, `home-quick-stats-grid`, `home-deck-grid` | `home-blueprint-theme.css` |
| `QuickStatTile.razor` | `home-quick-stat`, `home-quick-stat--up`, `home-quick-stat--down`, `home-quick-stat--flat`, `home-quick-stat__label`, `home-quick-stat__value`, `home-quick-stat__trend` | `home-blueprint-theme.css` |
| `DeckCard.razor` | `home-deck-card`, `home-deck-card__cover`, `home-deck-card__cover-fallback`, `home-deck-card__cover-token`, `home-deck-card__content`, `home-deck-card__title`, `home-deck-card__subtitle`, `home-deck-card__meta`, `home-deck-card__status`, `home-deck-card__progress`, `home-deck-card__action` | `home-blueprint-theme.css` |
| `HomeSideNav.razor` | `home-side-nav`, `home-side-nav__list`, `home-side-nav__item`, `home-side-nav__item--active` | `home-blueprint-theme.css` |

### Auth Components (Ui.Shared/Components/Auth/)

| Component | CSS Classes Used | Source File |
|-----------|-----------------|-------------|
| `LoginButton.razor` | `auth-login-panel`, `auth-login-card`, `auth-login-header`, `auth-login-title`, `auth-login-subtitle`, `auth-login-providers`, `auth-provider-btn`, `auth-provider-btn--google`, `auth-provider-btn--github`, `auth-provider-icon`, `auth-provider-label`, `auth-login-error` | **NOT YET DEFINED** — classes exist in Razor but have no matching CSS rules |
| `UserMenu.razor` | `auth-user-menu`, `auth-user-menu__trigger`, `auth-user-menu__avatar`, `auth-user-menu__avatar-fallback`, `auth-user-menu__name`, `auth-user-menu__chevron`, `auth-user-menu__chevron--open`, `auth-user-menu__dropdown`, `auth-user-menu__dropdown-header`, `auth-user-menu__dropdown-email`, `auth-user-menu__dropdown-actions`, `auth-user-menu__dropdown-btn`, `auth-user-menu__dropdown-btn--danger` | **NOT YET DEFINED** — classes exist in Razor but have no matching CSS rules |

### Web-Client Components

| Component | CSS Classes Used | Source File |
|-----------|-----------------|-------------|
| `ReconnectModal.razor` | `components-reconnect-*`, `#components-reconnect-modal` | `ReconnectModal.razor.css` (scoped) |

---

## Naming Convention Analysis

### Current Pattern: BEM-like (Block__Element--Modifier)

The project uses a **consistent BEM-like naming convention** across all CSS:
- **Blocks:** `home-section`, `sidebar-nav-link`, `auth-card`, `app-sidebar`
- **Elements:** `home-section__header`, `sidebar-nav-link__icon`, `auth-card__title`
- **Modifiers:** `home-quick-stat--up`, `sidebar-nav-link--active`, `app-sidebar--open`

**Domain prefixes provide logical grouping:**
- `home-*` → Home dashboard
- `sidebar-*`, `app-sidebar-*` → Sidebar layout
- `auth-*` → Authentication pages
- `pinned-decks-*` → Pinned decks widget
- `layout-*` → Shell layout

### Strengths
- ✅ Consistent BEM naming across all files
- ✅ Domain-scoped prefixes prevent accidental collisions
- ✅ Clear component ownership from class names
- ✅ No CSS specificity wars (all selectors are single-class)

### Weaknesses
- ❌ All classes are **global** — no encapsulation
- ❌ No build-time guarantee that a `.razor.css` file matches its component
- ❌ Styles can leak across components if prefixes overlap
- ❌ Dead code detection is manual — unused classes persist in global sheets

---

## Design Token System

The project uses a **comprehensive CSS custom properties (variables) system** defined in `home-blueprint-theme.css`:

### Token Categories
| Category | Variables | Example |
|----------|-----------|---------|
| **shadcn/ui base** | 15+ | `--background`, `--foreground`, `--card`, `--primary` |
| **Sidebar** | 7 | `--sidebar-background`, `--sidebar-primary`, `--sidebar-border` |
| **Claymorphism** | 12+ | `--clay-surface`, `--clay-shadow-card`, `--clay-accent` |
| **Claymorphism shadows** | 5 | `--clay-shadow-light`, `--clay-shadow-dark`, `--clay-shadow-outer` |

**Assessment:** The token system is well-designed and follows shadcn/ui conventions. Tokens are defined in `:root` and consumed globally. This is the ONE area where global scope is appropriate and should be preserved.

---

## Critical Issues Identified

### 1. **Zero Component Scoping**
Only `ReconnectModal.razor.css` uses Blazor's scoped CSS. All other components rely on global stylesheets. This means:
- Any developer can accidentally create a class that conflicts with existing styles
- Refactoring a component's markup requires auditing global CSS files
- Unused CSS cannot be tree-shaken

### 2. **Orphaned CSS Classes (LoginButton, UserMenu)**
`LoginButton.razor` and `UserMenu.razor` reference CSS classes (`auth-login-*`, `auth-user-menu-*`) that have **no corresponding CSS rules** in any stylesheet. These components will render unstyled.

### 3. **Inconsistent CSS Loading Strategy**
- `App.razor` loads most CSS via `<link>` tags
- `MainLayout.razor` loads `sidebar-layout.css` inline
- No CSS bundling or build-time optimization

### 4. **Global `body` Rules in Multiple Files**
Both `sidebar-layout.css` and `home-blueprint-theme.css` set `body` styles:
```css
/* sidebar-layout.css */
body { font-family: 'Inter', ...; -webkit-font-smoothing: antialiased; }

/* home-blueprint-theme.css */
body { background: var(--background); color: var(--foreground); }

/* Ui.Web/wwwroot/css/app.css */
html, body { margin: 0; padding: 0; min-height: 100%; font-family: 'Inter', ...; }
```
This creates fragile layering dependencies.

### 5. **Global `#blazor-error-ui` Defined Twice**
Both `Ui.Shared/wwwroot/app.css` and `Ui.Web/wwwroot/css/app.css` define `#blazor-error-ui`, with slightly different styles. Whichever loads last wins.

### 6. **Media Queries Scattered Across Files**
Responsive breakpoints appear in `sidebar-layout.css` (767px, 768px, 1023px) and `home-blueprint-theme.css` (720px), with no centralized breakpoint token system.

---

## Component-to-CSS Mapping Summary

```
┌─────────────────────────────────────────────────────────────────────┐
│                     GLOBAL STYLESHEETS                              │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  home-blueprint-theme.css                                          │
│  ├── :root (design tokens) ← KEEP GLOBAL                           │
│  ├── body ← CONSOLIDATE                                            │
│  ├── .home-shell, .home-main, .home-welcome                        │
│  ├── .home-section, .home-section__header                          │
│  ├── .home-quick-stat, .home-quick-stat--up/down                   │
│  ├── .home-deck-card, .home-deck-card__*                           │
│  └── @media (max-width: 720px)                                     │
│                                                                     │
│  sidebar-layout.css                                                │
│  ├── body ← CONSOLIDATE                                            │
│  ├── @media (prefers-reduced-motion) ← KEEP GLOBAL                 │
│  ├── .layout-shell, .layout-content                                │
│  ├── .app-sidebar, .app-sidebar--open, .app-sidebar__*             │
│  ├── .sidebar-nav-link, .sidebar-nav-link--active                  │
│  ├── .sidebar-section, .sidebar-section__*                         │
│  ├── .sidebar-quick-links                                          │
│  ├── .sidebar-user, .sidebar-user__*                               │
│  ├── .pinned-decks__*                                              │
│  ├── .sidebar-backdrop, .sidebar-toggle                            │
│  └── @media (max-width: 767px), @media (768px-1023px)             │
│                                                                     │
│  auth.css                                                          │
│  ├── .auth-layout                                                  │
│  ├── .auth-card, .auth-card__*                                     │
│  ├── .auth-provider-btn, .auth-provider-btn__*                     │
│  ├── .auth-form, .auth-form__*                                     │
│  ├── .auth-divider, .auth-submit-btn                               │
│  ├── .auth-error, .auth-terms                                      │
│  └── @keyframes auth-spin                                          │
│                                                                     │
│  app.css (Ui.Shared)                                               │
│  ├── .valid, .invalid, .validation-message ← KEEP GLOBAL           │
│  ├── .blazor-error-boundary ← KEEP GLOBAL                          │
│  ├── #blazor-error-ui ← DUPLICATE, CONSOLIDATE                     │
│  └── .form-floating ← KEEP GLOBAL (Bootstrap override)             │
│                                                                     │
│  app.css (Ui.Web)                                                  │
│  ├── html, body ← CONSOLIDATE                                      │
│  ├── .auth-guard-loading, .auth-guard-redirect                     │
│  └── #blazor-error-ui ← DUPLICATE, CONSOLIDATE                     │
│                                                                     │
│  app.css (Ui - MAUI)                                               │
│  └── .status-bar-safe-area ← KEEP GLOBAL (platform-specific)       │
│                                                                     │
├─────────────────────────────────────────────────────────────────────┤
│                     SCOPED STYLESHEETS (current)                    │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ReconnectModal.razor.css ← ONLY SCOPED FILE                       │
│  └── #components-reconnect-modal, .components-reconnect-*           │
│                                                                     │
├─────────────────────────────────────────────────────────────────────┤
│                     MISSING CSS (orphaned classes)                  │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  LoginButton.razor → auth-login-*, auth-provider-btn (partial)      │
│  UserMenu.razor → auth-user-menu-*                                  │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

---

## Recommendations Summary

1. **Adopt Blazor scoped CSS (`.razor.css`)** for all component-specific styles
2. **Preserve global scope** for: design tokens (`:root`), `body` reset, Blazor system classes, reduced-motion
3. **Consolidate duplicate rules** (`#blazor-error-ui`, `body` styles)
4. **Fix orphaned CSS** for `LoginButton.razor` and `UserMenu.razor`
5. **Centralize CSS loading** in `App.razor` instead of mixing strategies
6. **Create breakpoint tokens** for consistent responsive design
