# CSS Cleanup Plan for Mnemi App

## Executive Summary

This document outlines a plan to remove unused and deprecated CSS from the Mnemi application while maintaining UI functionality and following the shared UI architecture guidelines.

## Current CSS Structure

### CSS Files in the Project

1. **Shared CSS (Ui.Shared/wwwroot)**
   - `app.css` - Generic Bootstrap-like styles (likely legacy)
   - `Styles/sidebar-layout.css` - Modern sidebar layout system
   - `Styles/home-blueprint-theme.css` - Claymorphism theme (contains deprecated styles)
   - `Styles/auth-components.css` - Authentication components (not currently used)

2. **Web-specific CSS (Ui.Web/wwwroot)**
   - `css/app.css` - Web app overrides
   - `css/auth.css` - Auth page styles (currently in use)

3. **Component-specific CSS (Razor CSS files)**
   - `Layout/NavMenu.razor.css` - Legacy navigation menu
   - `Layout/MainLayout.razor.css` - Legacy main layout
   - `Layout/ReconnectModal.razor.css` - Reconnection modal (in use)

## Analysis Findings

### 1. Deprecated CSS - Safe to Remove

#### a) `home-blueprint-theme.css` - `.home-sidenav` section (Lines 95-127)
- **Status**: Marked as "Deprecated: Home Side Navigation (kept for backward compat)"
- **Usage**: No references found in any `.razor` or `.cs` files
- **Impact**: Zero - these styles are unused
- **Action**: Remove lines 95-127 and related responsive styles

#### b) `app.css` in Ui.Shared/wwwroot (Legacy Bootstrap styles)
- **Status**: Generic Bootstrap-like styles that conflict with modern theme
- **Usage**: Not referenced in current components
- **Impact**: Low - these styles are likely overridden by modern theme
- **Action**: Remove or keep only essential overrides if needed

#### c) `auth-components.css`
- **Status**: Authentication component styles
- **Usage**: Not referenced in any `.razor` files
- **Impact**: Zero - auth styles are in `auth.css` instead
- **Action**: Remove file entirely

### 2. Potentially Unused CSS - Needs Verification

#### a) `NavMenu.razor.css`
- **Status**: Legacy navigation menu styles
- **Usage**: NavMenu component exists but may use different styling
- **Impact**: Medium - needs verification
- **Action**: Check if NavMenu component uses these styles

#### b) `MainLayout.razor.css`
- **Status**: Legacy main layout styles
- **Usage**: MainLayout component exists but may use different styling
- **Impact**: Medium - needs verification
- **Action**: Check if MainLayout component uses these styles

### 3. CSS Files to Keep

#### a) `sidebar-layout.css` - **KEEP**
- **Reason**: Core layout system used by the app
- **Usage**: Referenced in `MainLayout.razor` and `App.razor`

#### b) `home-blueprint-theme.css` - **KEEP (partial)**
- **Reason**: Contains essential theme variables and modern styles
- **Action**: Remove only deprecated `.home-sidenav` section

#### c) `auth.css` (Ui.Web/wwwroot) - **KEEP**
- **Reason**: Currently used by Login and Signup pages

#### d) `ReconnectModal.razor.css` - **KEEP**
- **Reason**: Used by ReconnectModal component

## Implementation Plan

### Phase 1: Remove Confirmed Unused CSS ✅ COMPLETED

1. **Remove deprecated `.home-sidenav` section** ✅
   - File: `src/App/Ui.Shared/wwwroot/Styles/home-blueprint-theme.css`
   - Removed lines 95-127 (`.home-sidenav` and related classes)
   - Removed responsive styles for `.home-sidenav` (around line 288)
   - **Status**: Completed successfully

2. **Remove unused `auth-components.css` file** ✅
   - Deleted: `src/App/Ui.Shared/wwwroot/Styles/auth-components.css`
   - No references found in any files
   - **Status**: Completed successfully

3. **Review and clean up `app.css` in Ui.Shared** ✅
   - Removed generic Bootstrap-like styles (font-family, btn-primary, focus styles, etc.)
   - Kept essential Blazor-specific overrides (validation styles, error boundary, form floating labels)
   - **Status**: Completed successfully

### Phase 2: Verify Component CSS Pending

### Phase 2: Verify Component CSS Usage

1. **Check NavMenu.razor.css**
   - Review NavMenu.razor component
   - Determine if CSS classes are used
   - Remove if unused

2. **Check MainLayout.razor.css**
   - Review MainLayout.razor component
   - Determine if CSS classes are used
   - Remove if unused

### Phase 3: Verification Steps

1. **Build the application**
   ```bash
   dotnet build src/Ui.Web/Ui.Web.csproj
   dotnet build src/Ui.Maui/Ui.Maui.csproj
   ```

2. **Run the web application**
   ```bash
   dotnet run --project src/Ui.Web/Ui.Web.csproj
   ```

3. **Test key UI flows**
   - Home page
   - Authentication pages
   - Sidebar navigation
   - Reconnection modal

4. **Check browser console for CSS errors**

## Verification Checklist

### Phase 1 ✅ COMPLETED
- [x] All deprecated CSS removed
- [x] Application builds successfully
- [x] Web app runs without errors
- [x] UI appears correct in browser
- [x] No CSS-related console errors
- [x] Responsive design maintained
- [x] Authentication pages render correctly
- [x] Sidebar navigation works as expected

### Phase 2 Pending
- [ ] NavMenu.razor.css usage verified
- [ ] MainLayout.razor.css usage verified

## Risk Mitigation

1. **Backup before changes**
   - Commit current state to Git
   - Create feature branch for changes

2. **Incremental changes**
   - Remove CSS in small batches
   - Test after each change

3. **Rollback plan**
   - Keep Git history for easy rollback
   - Document all changes made

## Estimated Impact

- **Files to remove**: 1-2 files
- **Lines to remove**: ~50-100 lines
- **Build time impact**: Minimal (smaller CSS bundles)
- **Runtime impact**: Positive (faster CSS parsing)
- **Maintenance impact**: Positive (cleaner codebase)

## Follow-up Actions

1. Update documentation to reflect CSS structure
2. Add CSS linting to CI/CD pipeline
3. Consider CSS minification for production
4. Document CSS naming conventions
