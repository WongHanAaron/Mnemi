import { test, expect } from '@playwright/test';

test('home layout exposes required shell selectors on desktop', async ({ page }) => {
  await page.setViewportSize({ width: 1440, height: 900 });
  await page.goto('/home');

  await expect(page.getByTestId('home-shell')).toBeVisible();
  await expect(page.getByTestId('home-sidenav')).toBeVisible();
  await expect(page.getByTestId('home-welcome')).toBeVisible();
  await expect(page.getByTestId('home-quick-stats')).toBeVisible();
  await expect(page.getByTestId('home-recent-decks')).toBeVisible();
  await expect(page.getByTestId('home-pinned-decks')).toBeVisible();
});
