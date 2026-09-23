import type { Page } from "@playwright/test";

/**
 * Opens a page and waits until it is interactive. The panel is server rendered, so its buttons
 * are on screen before React hydrates them - a click that early submits the login form natively
 * (as a GET, with the password in the url) or does nothing at all. Idle network is the moment the
 * client bundle has loaded and taken over.
 */
export async function open(page: Page, url: string) {
	await page.goto(url);
	await page.waitForLoadState("networkidle");
}
