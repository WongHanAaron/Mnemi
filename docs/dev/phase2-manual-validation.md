# Phase 2 Manual Validation Checklist

**Date:** 2026-05-19  
**Build Status:** ✅ Success (9 warnings, all pre-existing)

## Prerequisites

- [x] Project builds successfully
- [ ] Browser with DevTools (Chrome, Edge, or Firefox recommended)
- [ ] Device toolbar enabled for mobile testing

---

## 1. Duplicate CSS Removal Validation

### 1.1 Verify No Duplicate Stylesheet Loading

**Steps:**
1. Open the application in your browser
2. Open DevTools (F12)
3. Go to **Network** tab
4. Filter by **CSS** or type `sidebar-layout.css`
5. Refresh the page (Ctrl+F5)

**Expected Result:**
- `sidebar-layout.css` should load **only once**
- Look for a single request to `_content/Ui.Shared/Styles/sidebar-layout.css`

**Pass Criteria:**
- [ ] Only one request for `sidebar-layout.css` appears in Network tab
- [ ] No 404 errors for CSS files

### 1.2 Verify Layout Rendering

**Steps:**
1. Navigate to the main page
2. Inspect the layout structure in DevTools **Elements** tab

**Expected Result:**
- Sidebar appears on the left (desktop) or hidden (mobile)
- Main content area renders correctly
- No visual glitches or layout shifts

**Pass Criteria:**
- [ ] Desktop: Sidebar visible with correct width (16rem)
- [ ] Desktop: Content area takes remaining space
- [ ] No console errors related to CSS

---

## 2. Accessibility Features Validation

### 2.1 Focus Visible on Sidebar Toggle (Mobile)

**Steps:**
1. Open DevTools **Device Toolbar** (Ctrl+Shift+M)
2. Select a mobile device (e.g., iPhone 12, 390x844)
3. Refresh the page
4. Press **Tab** key repeatedly until the hamburger menu button receives focus
5. Alternatively, click the sidebar toggle with mouse, then press Tab

**Expected Result:**
- A clear focus outline appears around the toggle button
- Outline should be 2px solid with primary color (blue: #0ea5e9)
- Outline offset of 2px from the button edge

**Pass Criteria:**
- [ ] Focus outline is clearly visible
- [ ] Outline color matches theme primary color
- [ ] Outline does not clip or overflow unexpectedly
- [ ] Focus order is logical (toggle button is reachable via Tab)

**Visual Check:**
```
Expected focus indicator:
┌─────────────┐
│  ╔═══════╗  │  ← 2px outline
│  ║  ☰   ║  │     with 2px offset
│  ╚═══════╝  │
└─────────────┘
```

### 2.2 Semantic HTML for Quick Links

**Steps:**
1. In DevTools **Elements** tab, locate the quick links section
2. Look for the `<nav>` element with class `sidebar-quick-links`
3. Verify the structure

**Expected HTML Structure:**
```html
<nav class="sidebar-quick-links" aria-label="Quick links" data-testid="sidebar-quick-links">
    <ul>
        <li>
            <SidebarNavLink ... />
        </li>
        <li>
            <SidebarNavLink ... />
        </li>
        <!-- more items -->
    </ul>
</nav>
```

**Pass Criteria:**
- [ ] `<nav>` element present with `aria-label="Quick links"`
- [ ] Quick links wrapped in `<ul>` element
- [ ] Each link wrapped in `<li>` element
- [ ] No `<div>` wrapper around the list
- [ ] Screen reader announces "Quick links" navigation region

**Screen Reader Test:**
- [ ] NVDA/JAWS announces "Quick links, navigation" when focusing the region
- [ ] List structure is announced correctly

### 2.3 Reduced Motion Support

**Steps:**
1. Open DevTools
2. **Chrome/Edge:** Go to **Rendering** tab → Check "Emulate CSS media feature prefers-reduced-motion"
   **Firefox:** Go to **Responsive Design Mode** → Enable "Reduce motion"
3. Refresh the page
4. Interact with collapsible sidebar sections
5. Toggle the sidebar on mobile

**Expected Result:**
- All animations and transitions should be disabled
- Sidebar opens/closes instantly
- Section expand/collapse happens instantly
- No smooth scrolling

**Pass Criteria:**
- [ ] Sidebar toggle animation is instant (no slide)
- [ ] Section expand/collapse is instant (no fade/slide)
- [ ] No visible transition effects
- [ ] Page scrolls instantly (no smooth scroll)

**Alternative Test (System Settings):**
- **Windows:** Settings → Ease of Access → Display → "Show animations" → OFF
- **macOS:** System Preferences → Accessibility → Display → "Reduce motion" → ON
- Restart browser and verify animations are disabled

---

## 3. Visual Regression Testing

### 3.1 Desktop View (≥1024px)

**Steps:**
1. Set browser width to 1200px or larger
2. Navigate through different pages

**Pass Criteria:**
- [ ] Sidebar is visible and fixed on the left
- [ ] Quick links appear at top of sidebar
- [ ] Pinned Decks section is collapsible
- [ ] All sections render with correct spacing
- [ ] No horizontal scrollbars
- [ ] Content area starts after sidebar (no overlap)

### 3.2 Tablet View (768px - 1023px)

**Steps:**
1. Set browser width to 800px
2. Check sidebar behavior

**Pass Criteria:**
- [ ] Sidebar is hidden by default
- [ ] Hamburger toggle button appears in top-left
- [ ] Toggle button is properly positioned (0.75rem from top/left)
- [ ] Clicking toggle opens sidebar as overlay
- [ ] Sidebar can be closed via toggle or outside click

### 3.3 Mobile View (<768px)

**Steps:**
1. Set browser width to 375px (iPhone SE)
2. Test all interactions

**Pass Criteria:**
- [ ] Hamburger toggle button is visible and tappable
- [ ] Button size is adequate (2.25rem × 2.25rem)
- [ ] Sidebar overlay covers full screen or appropriate width
- [ ] Touch targets are large enough (≥44px)
- [ ] No horizontal scrolling
- [ ] Content is readable without zooming

---

## 4. Cross-Browser Testing

Test the following in each browser:

### Chrome/Edge
- [ ] Focus indicators render correctly
- [ ] Reduced motion respected
- [ ] Semantic HTML structure correct
- [ ] Mobile toggle works

### Firefox
- [ ] Focus indicators render correctly
- [ ] Reduced motion respected
- [ ] Semantic HTML structure correct
- [ ] Mobile toggle works

### Safari (if available)
- [ ] Focus indicators render correctly
- [ ] Reduced motion respected
- [ ] Semantic HTML structure correct
- [ ] Mobile toggle works

---

## 5. Accessibility Tools Testing

### 5.1 Lighthouse Accessibility Audit

**Steps:**
1. Open DevTools → **Lighthouse** tab
2. Select **Accessibility** category
3. Run audit

**Expected Result:**
- No new accessibility issues introduced
- Score should be ≥90 (or improved from baseline)

**Pass Criteria:**
- [ ] "Buttons do not have accessible names" - No issues
- [ ] "Heading levels increase by one" - No