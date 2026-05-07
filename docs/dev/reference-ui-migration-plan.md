# Reference UI Migration Plan

## Overview

Migrate the reference UI from `reference/src/Ui.Web` and `reference/src/Ui.Shared` into the current `src/App/Ui.*` projects. The reference uses a **Blazor Server** pattern (net8.0) while the current project uses a **Blazor Web App with WASM** pattern (net10.0).

## Architecture Gap Analysis

| Aspect | Reference (`reference/src/Ui.Web`) | Current (`src/App/Ui.*`) |
|---|---|---|
| **Host model** | Blazor Server (`_Host.cshtml` + `blazor.server.js`) | Blazor Web App (`App.razor` component + SSR + WASM) |
| **Target framework** | net8.0 | net10.0 |
| **Auth mechanism** | Cookie auth + OAuth server redirects | None currently |
| **Layout** | `MainLayout` with auth guard + `AppSidebar` | Simple `MainLayout` + `NavMenu` |
| **Shared components** | Rich: `HomeDashboard`, `AuthGuard`, `Sidebar`, `UserMenu`, etc. | Boilerplate: `Counter`, `Weather` |
| **CSS** | `sidebar-layout.css`, `home-blueprint-theme.css`, `auth.css`, `auth-components.css` | `app.css` only |
| **Favicons** | In `wwwroot/favicon/` subfolder | In `wwwroot/` root |

---

## Phase 1: Stylesheets & Static Assets Migration

**Goal:** Bring over all CSS files and favicons without any component changes. The app looks the same functionally but gains the style foundation.

### Files to migrate

| From (reference) | To |
|---|---|
| `reference/src/Ui.Web/wwwroot/css/app.css` | `src/App/Ui.Web/wwwroot/css/app.css` |
| `reference/src/Ui.Web/wwwroot/css/auth.css` | `src/App/Ui.Web/wwwroot/css/auth.css` |
| `reference/src/Ui.Shared/wwwroot/Styles/sidebar-layout.css` | `src/App/Ui.Shared/wwwroot/Styles/sidebar-layout.css` |
| `reference/src/Ui.Shared/wwwroot/Styles/home-blueprint-theme.css` | `src/App/Ui.Shared/wwwroot/Styles/home-blueprint-theme.css` |
| `reference/src/Ui.Shared/wwwroot/Styles/auth-components.css` | `src/App/Ui.Shared/wwwroot/Styles/auth-components.css` |
| `reference/src/Ui.Web/wwwroot/favicon/*` | `src/App/Ui.Web/wwwroot/favicon/*` |

### Verification (manual + MCP)

1. `dotnet build src/App/Ui.Web/Ui.Web.csproj` — succeeds with no errors
2. Open `https://localhost:5001` in browser
3. **MCP verifiable:** Check that `<link>` tags in page source reference the new CSS files
4. **MCP verifiable:** Open DevTools → Network tab, reload page, confirm all CSS files load with HTTP 200
5. **Visual:** App maintains existing look (no regressions) since components haven't changed yet

---

## Phase 2: Ui.Shared Models & Services Migration

**Goal:** Add the shared data models, service interfaces, and auth primitives to `Ui.Shared` without changing any UI components.

### Files to create/copy

| File | Destination |
|---|---|
| `reference/src/Ui.Shared/Services/IAuthService.cs` | `src/App/Ui.Shared/Services/IAuthService.cs` |
| `reference/src/Ui.Shared/Services/AuthState.cs` (incl. AuthResult, AuthProvider) | `src/App/Ui.Shared/Services/AuthState.cs` |
| `reference/src/Ui.Shared/Ports/IViewStateService.cs` | `src/App/Ui.Shared/Ports/IViewStateService.cs` |
| `reference/src/Ui.Shared/Models/ViewState.cs` | `src/App/Ui.Shared/Models/ViewState.cs` |
| `reference/src/Ui.Shared/Models/PinnedDeckItem.cs` | `src/App/Ui.Shared/Models/PinnedDeckItem.cs` |
| `reference/src/Ui.Shared/Models/SidebarNavItem.cs` | `src/App/Ui.Shared/Models/SidebarNavItem.cs` |
| `reference/src/Ui.Shared/Models/Home/*` (all 9 files) | `src/App/Ui.Shared/Models/Home/*` |

### Project file updates

- **`Ui.Shared.csproj`:** Add `<ProjectReference>` to `Application.csproj` (needed by `IAuthService`)
- **`_Imports.razor`:** Add `@using Mnemi.Ui.Shared.Models.Home` and `@using Mnemi.Ui.Shared.Services`

### Verification (manual + MCP)

1. `dotnet build src/App/Ui.Shared/Ui.Shared.csproj` — succeeds
2. `dotnet build src/App/Ui.Web/Ui.Web.csproj` — succeeds (ensures no breaking changes)
3. **MCP verifiable:** Navigate to `https://localhost:5001`, check browser console for zero errors
4. **Visual:** Existing pages (Home, Counter, Weather) still render correctly

---

## Phase 3: Layout Components Migration

**Goal:** Replace the boilerplate `MainLayout` and `NavMenu` with the reference `AppSidebar`-based layout. This is the biggest visual change.

### Files to create/copy

| From (reference) | To |
|---|---|
| `reference/src/Ui.Shared/Components/Layout/AppSidebar.razor` | `src/App/Ui.Shared/Components/Layout/AppSidebar.razor` |
| `reference/src/Ui.Shared/Components/Layout/SidebarNavLink.razor` | `src/App/Ui.Shared/Components/Layout/SidebarNavLink.razor` |
| `reference/src/Ui.Shared/Components/Layout/SidebarPinnedDecks.razor` | `src/App/Ui.Shared/Components/Layout/SidebarPinnedDecks.razor` |
| `reference/src/Ui.Shared/Components/Layout/SidebarQuickLinks.razor` | `src/App/Ui.Shared/Components/Layout/SidebarQuickLinks.razor` |
| `reference/src/Ui.Shared/Components/Layout/SidebarSection.razor` | `src/App/Ui.Shared/Components/Layout/SidebarSection.razor` |
| `reference/src/Ui.Shared/Components/Layout/SidebarUserSection.razor` | `src/App/Ui.Shared/Components/Layout/SidebarUserSection.razor` |

### Component updates

- **`src/App/Ui.Shared/Layout/MainLayout.razor`:** Replace current content with the reference layout (sidebar + content area). Remove the auth guard for now — show the sidebar unconditionally.
- **`src/App/Ui.Shared/Layout/NavMenu.razor`:** Can be deleted or kept empty (sidebar replaces it).

### Verification (manual + MCP)

1. `dotnet build` — all projects compile
2. Open `https://localhost:5001`
3. **Visual:** Sidebar appears on the left with Home, Review, Decks quick links
4. **Visual:** Sidebar has a toggle button (hamburger) on mobile widths
5. **MCP verifiable:** Check that `<aside class="app-sidebar">` exists in the DOM using `document.querySelector('.app-sidebar')`
6. **MCP verifiable:** Check `data-testid="sidebar"` element is rendered
7. **MCP verifiable:** Check `data-testid="sidebar-nav-link-home"` has `aria-current="page"` when on `/`
8. **Visual:** Existing pages (`/counter`, `/weather`) still navigate correctly via sidebar links
9. **Visual:** On phone-width viewport (< 768px), sidebar collapses and toggle is visible

---

## Phase 4: Home Dashboard Components Migration

**Goal:** Replace the boilerplate `Home.razor` with the rich dashboard UI.

### Files to create/copy

| From (reference) | To |
|---|---|
| `reference/src/Ui.Shared/Components/Home/HomeDashboard.razor` | `src/App/Ui.Shared/Components/Home/HomeDashboard.razor` |
| `reference/src/Ui.Shared/Components/Home/DeckCard.razor` | `src/App/Ui.Shared/Components/Home/DeckCard.razor` |
| `reference/src/Ui.Shared/Components/Home/QuickStatTile.razor` | `src/App/Ui.Shared/Components/Home/QuickStatTile.razor` |
| `reference/src/Ui.Shared/Components/Home/HomeSideNav.razor` | `src/App/Ui.Shared/Components/Home/HomeSideNav.razor` |
| `reference/src/Ui.Web/Services/HomeDashboardService.cs` | `src/App/Ui.Web/Services/HomeDashboardService.cs` |
| `reference/src/Ui.Web/Services/HomeDashboardStubDataProvider.cs` | `src/App/Ui.Web/Services/HomeDashboardStubDataProvider.cs` |

### Component updates

- **`src/App/Ui.Shared/Pages/Home.razor`:** Replace with the reference `Home.razor` that uses `HomeDashboard` component with stub data.
- **`src/App/Ui.Web/Program.cs`:** Register `IHomeDashboardService` → `HomeDashboardService` and `HomeDashboardStubDataProvider` as scoped services.

### Verification (manual + MCP)

1. `dotnet build` — compiles
2. Open `https://localhost:5001`
3. **Visual:** Home page shows "Welcome back, Learner" greeting
4. **Visual:** "Study now" primary action button is visible
5. **Visual:** Three sections: "Quick stats", "Recent decks", "Pinned decks" with loading/empty states
6. **Visual:** Deck cards appear with title, subtitle, progress bar, and action button
7. **MCP verifiable:** `document.querySelector('[data-testid="home-shell"]')` exists
8. **MCP verifiable:** `document.querySelector('[data-testid="home-primary-study-action"]')` exists and has text "Start studying"
9. **MCP verifiable:** `document.querySelectorAll('[data-testid^="deck-card-"]')` returns at least 2 deck cards
10. **MCP verifiable:** `document.querySelectorAll('[data-testid^="quick-stat-"]')` returns 3 quick stat tiles

---

## Phase 5: Auth Infrastructure (Server Side)

**Goal:** Wire up OAuth authentication (Google + GitHub) in the Ui.Web server project. This is the most complex phase.

### Files to create/copy

| From (reference) | To |
|---|---|
| `reference/src/Ui.Web/Controllers/AuthController.cs` | `src/App/Ui.Web/Controllers/AuthController.cs` |
| `reference/src/Ui.Web/Services/WebAuthService.cs` | `src/App/Ui.Web/Services/WebAuthService.cs` |
| `reference/src/Ui.Web/AdapterServiceRegistrations.cs` | `src/App/Ui.Web/Services/WebTokenEncryptionService.cs` |

### Project file updates

- **`Ui.Web.csproj`:**
  - Add `Microsoft.AspNetCore.Authentication.Google` package
  - Add `ProjectReference` to `Application.csproj` and `Domain.csproj`
- **`Program.cs`:**
  - Add cookie + OAuth authentication (Google + GitHub)
  - Add controllers (`builder.Services.AddControllers()` + `app.MapControllers()`)
  - Register `ITokenEncryptionService` → `WebTokenEncryptionService`
  - Register `WebAuthService` and `IAuthService` as scoped services
- **`App.razor`:** Add `@rendermode="InteractiveServer"` to the `<HeadOutlet>` to enable auth state to flow through Blazor circuit.

### Verification (manual + MCP)

1. `dotnet build` — compiles
2. Run the app, navigate to `https://localhost:5001`
3. **MCP verifiable:** Navigate to `/api/auth/me` — returns `{"isAuthenticated":false}` (JSON)
4. **MCP verifiable:** Navigate to `/login` — page renders without 404
5. **MCP verifiable:** Click Google login button, verify redirect to Google OAuth
6. **Visual:** After OAuth flow, redirected back to home page
7. **MCP verifiable:** After login, `/api/auth/me` returns `{"isAuthenticated":true, ...}`
8. **MCP verifiable:** Navigate to `/api/auth/logout` — signs out, redirected to login

---

## Phase 6: Auth UI Components & Auth Guard

**Goal:** Wire the auth state into the UI — auth guard, login page, user menu.

### Files to create/copy

| From (reference) | To |
|---|---|
| `reference/src/Ui.Shared/Components/Auth/AuthGuard.razor` | `src/App/Ui.Shared/Components/Auth/AuthGuard.razor` |
| `reference/src/Ui.Shared/Components/Auth/LoginButton.razor` | `src/App/Ui.Shared/Components/Auth/LoginButton.razor` |
| `reference/src/Ui.Shared/Components/Auth/UserMenu.razor` | `src/App/Ui.Shared/Components/Auth/UserMenu.razor` |
| `reference/src/Ui.Web/Pages/Login.razor` | `src/App/Ui.Shared/Pages/Login.razor` |
| `reference/src/Ui.Web/Pages/Signup.razor` | `src/App/Ui.Shared/Pages/Signup.razor` |
| `reference/src/Ui.Web/Components/Layout/AuthLayout.razor` | `src/App/Ui.Shared/Components/Layout/AuthLayout.razor` |

### Component updates

- **`MainLayout.razor`:** Re-add the auth guard from Phase 3 — only show sidebar+content when authenticated, redirect to `/login` otherwise.
- **`AuthLayout.razor`:** Update namespaces from `Mnemi.Ui.Web.Components.Layout` to match current project structure.

### Verification (manual + MCP)

1. `dotnet build` — compiles
2. Clear cookies, navigate to `https://localhost:5001/`
3. **Visual:** Redirected to `/login` page with "Welcome back" heading
4. **Visual:** Login page shows Google + GitHub OAuth buttons
5. **Visual:** "Or continue with" divider with email/password form (disabled)
6. **Visual:** "Don't have an account? Sign up" link at bottom
7. **MCP verifiable:** `document.querySelector('[data-testid="auth-login-google"]')` exists
8. **MCP verifiable:** `document.querySelector('[data-testid="auth-login-github"]')` exists
9. **Visual:** After successful OAuth login, redirected to home dashboard
10. **Visual:** User avatar/initials appear in the sidebar user section
11. **MCP verifiable:** `document.querySelector('[data-testid="auth-user-menu"]')` exists when logged in
12. **Visual:** Click "Sign Out" in user menu → redirected to login page

---

## Phase 7: MAUI App Alignment

**Goal:** Ensure the MAUI app (`src/App/Ui/`) uses the same shared components. Since `Ui.Shared` is the single source of components, most of this should already work.

### Files to update

- **`src/App/Ui/wwwroot/index.html`:** Add references to the new CSS stylesheets
- **`src/App/Ui/MauiProgram.cs`:** Register `IViewStateService` with a MAUI-appropriate implementation (detect device form factor instead of browser viewport)
- **`src/App/Ui/Services/`:** Add MAUI implementations of `IAuthService` (using WebView cookie sharing or native auth)

### Verification (manual + MCP)

1. `dotnet build src/App/Ui/Ui.csproj -f net10.0-windows10.0.19041.0` — compiles
2. **Visual:** MAUI app shows the same sidebar layout as the web app
3. **Visual:** Home dashboard renders with stub data
4. **Visual:** Sidebar responsive behavior works on window resize

---

## Phase 8: Cleanup & Polish

**Goal:** Remove deprecated boilerplate files and ensure everything is consistent.

### Files to remove

- `src/App/Ui.Shared/Pages/Counter.razor` — demo page
- `src/App/Ui.Shared/Pages/Weather.razor` — demo page
- `src/App/Ui.Shared/Layout/NavMenu.razor` — replaced by AppSidebar
- `src/App/Ui.Shared/Layout/NavMenu.razor.css` — replaced by AppSidebar
- `src/App/Ui.Shared/Services/IFormFactor.cs` — if no longer needed

### Imports cleanup

Update `_Imports.razor` files across all projects — remove unused usings, add any missing ones.

### Verification (manual + MCP)

1. `dotnet build` — all projects compile with zero warnings
2. Run the web app, navigate all pages — no 404s or broken links
3. **MCP verifiable:** Run `dotnet test` for all test projects — all pass
4. **Visual:** App looks and feels complete with sidebar nav, home dashboard, auth flow
5. **MCP verifiable:** Lighthouse PWA audit — app is installable, manifest loads correctly

---

## Dependency Flow Diagram

```
Phase 1: Stylesheets ──► CSS foundation (no visual change yet)
    │
Phase 2: Models/Services ──► Data contracts + auth interfaces (no visual change)
    │
Phase 3: Layout ──► Sidebar replaces old nav (BIG visual change)
    │
Phase 4: Home Dashboard ──► Rich dashboard replaces boilerplate Home
    │
Phase 5: Auth Infrastructure ──► OAuth login/logout works server-side
    │
Phase 6: Auth UI ──► Login page, auth guard, user menu
    │
Phase 7: MAUI Alignment ──► Shared components work on mobile
    │
Phase 8: Cleanup ──► Remove boilerplate, final polish
```

Each phase is independently buildable and verifiable. The MCP-verifiable checks use `data-testid` attributes (which the reference components already have) and standard DOM queries.
