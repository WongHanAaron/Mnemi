# BlazorBlueprint.Components — Developer Usage Guide

**Version:** 3.10.2 | **License:** Apache 2.0 | **.NET:** 8.0+
**Docs:** [blazorblueprintui.com](https://blazorblueprintui.com/)

---

## Overview

BlazorBlueprint.Components is a pre-styled, shadcn/ui-compatible Blazor component library. It ships with pre-built CSS (no Tailwind required), includes `BlazorBlueprint.Primitives` (headless accessibility primitives) and `BlazorBlueprint.Icons.Lucide` (1000+ SVG icons) automatically.

This guide documents how to use it **within the Mnemi project**, including which components replace our custom ones, naming conventions, theming integration, and migration patterns.

---

## 1. Setup in Mnemi

### 1.1 Installation

```powershell
# In src/App/Ui.Shared/
dotnet add package BlazorBlueprint.Components --version 3.10.2
```

This auto-installs `BlazorBlueprint.Primitives` and `BlazorBlueprint.Icons.Lucide`.

### 1.2 Registration (Program.cs)

```csharp
// src/App/Ui.Web/Program.cs AND src/App/Ui/MauiProgram.cs
builder.Services.AddBlazorBlueprintComponents();
```

This registers: `ToastService`, `DialogService`, `IPortalService`, `IFocusManager`, `IPositioningService`, `IKeyboardShortcutService`, and `DropdownManagerService`.

### 1.3 Imports (_Imports.razor)

```razor
<!-- src/App/Ui.Shared/_Imports.razor -->
@using BlazorBlueprint.Components
@using BlazorBlueprint.Primitives
@using BlazorBlueprint.Icons.Lucide
```

### 1.4 CSS Loading (App.razor)

Add the Blueprint stylesheet **after** our design tokens but **before** our custom CSS:

```html
<!-- src/App/Ui.Web/Components/App.razor -->
<link rel="stylesheet" href="_content/Ui.Shared/Styles/tokens.css" />
<link rel="stylesheet" href="_content/BlazorBlueprint.Components/blazorblueprint.css" />
<link rel="stylesheet" href="_content/Ui.Shared/Styles/reset.css" />
<link rel="stylesheet" href="_content/Ui.Shared/app.css" />
```

**Critical ordering:** Our `tokens.css` defines CSS variables that Blueprint consumes. If Blueprint loads first, its defaults override our tokens.

### 1.5 Portal Host (MainLayout.razor)

Add `<BbPortalHost />` at the bottom of `MainLayout.razor`. Required for overlays (dialogs, toasts, dropdowns, tooltips) to render correctly:

```razor
<!-- src/App/Ui.Shared/Layout/MainLayout.razor -->
@Body
<BbPortalHost />
```

### 1.6 MAUI (index.html)

Add the same stylesheet to `src/App/Ui/wwwroot/index.html`:

```html
<link rel="stylesheet" href="_content/Ui.Shared/Styles/tokens.css" />
<link rel="stylesheet" href="_content/BlazorBlueprint.Components/blazorblueprint.css" />
```

---

## 2. Component Mapping — Custom → Blueprint

| Mnemi Custom | Blueprint Component | Migration Complexity |
|---|---|---|
| **Buttons** | | |
| `account-card__action-btn` | `BbButton Variant="Outline" Size="Small"` | Low |
| `account-card__action-btn--primary` | `BbButton Variant="Default" Size="Small"` | Low |
| `account-card__action-btn--secondary` | `BbButton Variant="Ghost" Size="Small"` | Low |
| `danger-zone__trigger-btn` | `BbButton Variant="Destructive"` | Low |
| `provider-row__unlink-btn` | `BbButton Variant="Ghost" Size="Small"` | Low |
| `source-row__action-btn` | `BbButton Variant="Ghost" Size="Small"` | Low |
| `source-row__action-btn--danger` | `BbButton Variant="Destructive" Size="Small"` | Low |
| `delete-dialog__cancel-btn` | `BbButton Variant="Outline"` | Low |
| `delete-dialog__delete-btn` | `BbButton Variant="Destructive" Disabled="true/false"` | Low |
| **Cards** | | |
| `CardLayout.razor` | `BbCard` + `BbCardHeader` + `BbCardContent` | Low |
| `AccountProfileCard.razor` | `BbCard` + `BbInput` + `BbButton` | Medium |
| **Dialogs** | | |
| `ConfirmDeleteDialog.razor` | `BbAlertDialog` | Low |
| **Skeletons** | | |
| `Skeleton.razor` | `BbSkeleton` (`Rectangular` / `Circular`) | Low |
| **Toasts** | | |
| `ToastNotification.razor` | `BbToast` (programmatic via `ToastService`) | Medium |
| **Badges** | | |
| `source-row__status`, `provider-row__status` | `BbBadge Variant="Default/Secondary/Destructive/Outline"` | Low |
| **Inputs** | | |
| `account-field__input`, `source-row__input` | `BbInput` (`Text`, `Email`, `Password`, etc.) | Low |
| **Avatars** | | |
| `SidebarUserSection` avatar | `BbAvatar` + `BbAvatarFallback` | Low |
| **Sidebar** | | |
| `AppSidebar.razor` | `BbSidebar` | High |
| `SidebarSection.razor` | `BbAccordion Type="AccordionType.Single"` | Medium |
| `SidebarNavLink.razor` | Custom with `BbButton Variant="Ghost"` | Medium |
| `SidebarIcon.razor` | `BbLucideIcon` (replace with named icons) | High |
| **Navigation** | | |
| `UserMenu.razor` | `BbDropdownMenu` | Medium |
| `HomeSideNav.razor` | `BbTabs` | Medium |
| **Data** | | |
| `DeckCard.razor` | `BbCard` + custom internals | Medium |
| `QuickStatTile.razor` | `BbCard` + custom metric display | Low |

---

## 3. Button Variants & Usage Patterns

### 3.1 Variant Guide

```
<BbButton Variant="ButtonVariant.Default">Primary action</BbButton>
<BbButton Variant="ButtonVariant.Destructive">Danger / delete</BbButton>
<BbButton Variant="ButtonVariant.Outline">Secondary / cancel</BbButton>
<BbButton Variant="ButtonVariant.Ghost">Subtle / inline / row actions</BbButton>
<BbButton Variant="ButtonVariant.Secondary">Alternative primary</BbButton>
<BbButton Variant="ButtonVariant.Link">Link-style button</BbButton>
```

### 3.2 Size Guide

```
<BbButton Size="ButtonSize.Small">Compact (row actions, cards)</BbButton>
<BbButton Size="ButtonSize.Default">Standard</BbButton>
<BbButton Size="ButtonSize.Large">Hero / CTA</BbButton>
<BbButton Size="ButtonSize.Icon">Icon only</BbButton>
```

### 3.3 Button → Mnemi Mapping Rules

| Context | Variant | Size | Example |
|---|---|---|---|
| Card header action (Edit, + Add, + Link New) | `Outline` | `Small` | `Edit` button on profile card |
| Primary card action (Save) | `Default` | `Small` | `Save` button in edit mode |
| Cancel action | `Ghost` | `Small` | `Cancel` button in edit mode |
| Row action (Edit source, Unlink provider) | `Ghost` | `Small` | Inline row buttons |
| Destructive row action (Remove) | `Ghost` + `Class="text-red-500"` | `Small` | Remove source button |
| Danger zone trigger | `Destructive` | `Default` | Delete My Account |
| Dialog cancel | `Outline` | `Default` | Cancel in confirm dialog |
| Dialog confirm delete | `Destructive` | `Default` | Permanently Delete Account |
| Navigation / sidebar link | `Ghost` | `Default` | Sidebar nav items |
| Hero / page action | `Default` | `Large` | Start Studying CTA |
| Icon-only (no text) | `Ghost` | `Icon` | Toolbar icon buttons |

---

## 4. Card Patterns

### 4.1 Basic Card

```razor
<BbCard>
    <BbCardHeader>
        <BbCardTitle>Section Title</BbCardTitle>
        <BbCardDescription>Optional subtitle</BbCardDescription>
    </BbCardHeader>
    <BbCardContent>
        @* Card body content *@
    </BbCardContent>
    <BbCardFooter>
        @* Footer actions *@
    </BbCardFooter>
</BbCard>
```

### 4.2 Card with Header Actions (Replaces CardLayout.razor)

Our `CardLayout.razor` pattern (title + header action buttons) maps to:

```razor
<BbCard>
    <BbCardHeader Class="flex-row items-center justify-between">
        <div>
            <BbCardTitle>👤 Profile</BbCardTitle>
        </div>
        <BbButton Variant="ButtonVariant.Outline" Size="ButtonSize.Small">Edit</BbButton>
    </BbCardHeader>
    <BbCardContent>
        @* Body *@
    </BbCardContent>
</BbCard>
```

### 4.3 Danger Variant Card

```razor
<BbCard Class="border-red-500/30">
    <BbCardHeader>
        <BbCardTitle Class="text-red-500">⚠️ Danger Zone</BbCardTitle>
    </BbCardHeader>
    @* ... *@
</BbCard>
```

---

## 5. Dialog Patterns

### 5.1 Alert Dialog (Replaces ConfirmDeleteDialog.razor)

```razor
<BbAlertDialog>
    <BbAlertDialogTrigger AsChild>
        <BbButton Variant="ButtonVariant.Destructive">Delete My Account</BbButton>
    </BbAlertDialogTrigger>
    <BbAlertDialogContent>
        <BbAlertDialogHeader>
            <BbAlertDialogTitle>⚠️ Delete Account</BbAlertDialogTitle>
            <BbAlertDialogDescription>
                This will permanently delete your account and ALL data.
                This action CANNOT be undone.
            </BbAlertDialogDescription>
        </BbAlertDialogHeader>
        <BbAlertDialogFooter>
            <BbAlertDialogCancel AsChild>
                <BbButton Variant="ButtonVariant.Outline">Cancel</BbButton>
            </BbAlertDialogCancel>
            <BbAlertDialogAction AsChild>
                <BbButton Variant="ButtonVariant.Destructive">⛔ Permanently Delete Account</BbButton>
            </BbAlertDialogAction>
        </BbAlertDialogFooter>
    </BbAlertDialogContent>
</BbAlertDialog>
```

### 5.2 Programmatic Toast (Replaces ToastNotification.razor)

```csharp
@inject ToastService ToastService

// Show toast
ToastService.Show("✓ Saved", "Save successful");

// Show destructive/toast variant
ToastService.Show("❌ Failed to save", "Error");
```

Remove our `ToastNotification.razor` component entirely — `ToastService` handles the portal rendering automatically via `<BbPortalHost />`.

---

## 6. Skeleton Loading (Replaces Skeleton.razor)

```razor
@* Rectangular (text, rows, blocks) *@
<BbSkeleton Shape="SkeletonShape.Rectangular" Class="w-full h-4" />
<BbSkeleton Shape="SkeletonShape.Rectangular" Class="w-3/4 h-4" />

@* Circular (avatars) *@
<BbSkeleton Shape="SkeletonShape.Circular" Class="w-12 h-12" />
```

Remove our `Skeleton.razor` and `Skeleton.razor.css` — all skeleton CSS is provided by the library.

---

## 7. Badges (Replaces status indicators)

```razor
<BbBadge Variant="BadgeVariant.Default">✅ Active</BbBadge>
<BbBadge Variant="BadgeVariant.Secondary">⚠️ Expired</BbBadge>
<BbBadge Variant="BadgeVariant.Destructive">⚠️ Error</BbBadge>
<BbBadge Variant="BadgeVariant.Outline">Google Drive</BbBadge>
```

---

## 8. Input Fields

```razor
@* Basic text input *@
<BbInput Type="InputType.Text" @bind-Value="_editValue" Placeholder="Display name" />

@* Email input *@
<BbInput Type="InputType.Email" @bind-Value="_email" Disabled="true" />

@* With field wrapper (label, description, error) *@
<BbFormFieldInput Label="Display Name"
                  Description="Your public display name"
                  Error="@_errorMessage"
                  @bind-Value="_editValue" />
```

---

## 9. Theming Integration

### 9.1 Token Compatibility

Our `tokens.css` already uses shadcn/ui-compatible CSS variable names:

| Mnemi Token | Blueprint Consumption | Notes |
|---|---|---|
| `--background` | ✅ Used directly | Matches shadcn |
| `--foreground` | ✅ Used directly | Matches shadcn |
| `--card` | ✅ Used directly | Matches shadcn |
| `--primary` | ✅ Used directly | Matches shadcn |
| `--secondary` | ✅ Used directly | Matches shadcn |
| `--muted` | ✅ Used directly | Matches shadcn |
| `--accent` | ✅ Used directly | Matches shadcn |
| `--destructive` | ✅ Used directly | Matches shadcn |
| `--border` | ✅ Used directly | Matches shadcn |
| `--radius` | ✅ Used directly | Matches shadcn |
| `--clay-accent` | ❌ Not consumed by Blueprint | Mnemi-specific |
| `--sidebar-*` | ❌ Not consumed by Blueprint | Mnemi-specific |

**Key insight:** Our design tokens are already shadcn/ui-compatible. Blueprint components will pick up our colors automatically. Custom `--clay-*` and `--sidebar-*` tokens are extended tokens that only our custom components use.

### 9.2 Adding Custom Tokens for Mnemi-Specific Styling

If a Blueprint component needs Mnemi-specific styling, use the `Class` parameter:

```razor
<BbCard Class="border-[var(--clay-border)]" />
<BbButton Class="bg-[var(--clay-accent)] text-white" />
```

### 9.3 Dark Mode

Blueprint automatically applies dark mode when `<html class="dark">`. Our `tokens.css` already has a `.dark` block — ensure it's consistent:

```css
:root { --background: #171717; }  /* Mnemi is dark by default */
.dark { --background: #171717; }  /* Match Blueprint's dark mode trigger */
```

---

## 10. Migration "Cheat Sheet"

When migrating a component:

1. **Remove** the custom `.razor.css` file for any styles fully replaced by Blueprint
2. **Remove** the `@* ... *@` CSS file header comment block
3. **Replace** `<div class="account-card">` → `<BbCard>`
4. **Replace** `<header class="account-card__header">` → `<BbCardHeader>`
5. **Replace** `<h2 class="account-card__title">` → `<BbCardTitle>`
6. **Replace** `<div class="account-card__body">` → `<BbCardContent>`
7. **Replace** any `<button>` → `<BbButton Variant="..." Size="...">`
8. **Replace** `data-testid` → keep them! Add `data-testid` attributes to Blueprint components via `AdditionalAttributes`
9. **Keep** `@code { }` blocks intact unless they manage UI state now handled by Blueprint
10. **Verify** E2E tests still pass — `data-testid` selectors must work with Blueprint components

---

## 11. Accessibility Notes

Blueprint components include built-in ARIA attributes and keyboard support from `BlazorBlueprint.Primitives`. When migrating:

- **Remove** manual `role="dialog"`, `aria-modal`, `aria-labelledby` — Blueprint provides them
- **Keep** `aria-hidden="true"` on decorative icons
- **Add** `aria-label` where Blueprint doesn't provide context
- **Verify** keyboard navigation: Tab, Enter, Escape, arrow keys all work

---

## 12. Common Pitfalls

1. **Portal host missing:** Dialogs, toasts, dropdowns won't render. Always have `<BbPortalHost />` in `MainLayout.razor`.
2. **CSS load order:** Our `tokens.css` must load BEFORE `blazorblueprint.css`.
3. **AsChild confusion:** `AsChild` merges trigger behavior into the child component. Only use when the trigger is a `BbButton` or similar Blueprint component.
4. **data-testid on Blueprint components:** Blueprint components might not pass `data-testid` through by default. Test this early in migration.
5. **Component state loss:** Replacing custom components with Blueprint may change how state is managed. Ensure `@bind-*` and `EventCallback` patterns still work.
6. **MAUI compatibility:** Blueprint is designed for web. Test on MAUI BlazorWebView before committing to migration.
