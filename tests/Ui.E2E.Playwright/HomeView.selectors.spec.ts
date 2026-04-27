import { test, expect } from '@playwright/test';

test('home selector contract remains stable', async ({ page }) => {
  await page.goto('/home');

  await expect(page.getByTestId('home-shell')).toBeVisible();
  await expect(page.getByTestId('home-primary-study-action')).toBeVisible();
  await expect(page.getByTestId('home-quick-stats')).toBeVisible();
  await expect(page.getByTestId('home-recent-decks')).toBeVisible();
  await expect(page.getByTestId('home-pinned-decks')).toBeVisible();

  await expect(page.locator('[data-testid^="deck-card-"]').first()).toBeVisible();
  await expect(page.locator('[data-testid^="deck-card-action-"]').first()).toBeVisible();
});
