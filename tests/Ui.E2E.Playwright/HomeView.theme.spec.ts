import { test, expect } from '@playwright/test';

test('home deck cards keep theme structure classes', async ({ page }) => {
  await page.setViewportSize({ width: 1440, height: 900 });
  await page.goto('/home');

  const firstDeckCard = page.locator('.home-deck-card').first();
  await expect(firstDeckCard).toBeVisible();
  await expect(firstDeckCard.locator('.home-deck-card__cover')).toBeVisible();
  await expect(firstDeckCard.locator('.home-deck-card__meta')).toBeVisible();
});
