import { fileURLToPath } from "node:url";
import type { Locator, Page } from "@playwright/test";

/** `<repo>/docs/screenshots` - the gallery the root Readme shows. */
const screenshotsDir = fileURLToPath(new URL("../../../docs/screenshots/", import.meta.url));

/** Toasts come and go on their own clock; they are feedback, not part of the screen. */
const hideTransient = "[data-sonner-toaster] { display: none !important; }";

/**
 * Writes a documentation screenshot, and only when asked to (`yarn e2e:screenshots` sets
 * SCREENSHOTS=1) - a plain test run must not leave the gallery modified in git.
 *
 * Waits for the network and the web fonts first, so the picture never shows a spinner or a
 * fallback font, scrolls to the top, moves the pointer out of the way and freezes animations
 * and the caret.
 */
export async function docShot(page: Page, name: string, options: { mask?: Locator[] } = {}) {
	if (process.env.SCREENSHOTS !== "1") {
		return;
	}

	await page.waitForLoadState("networkidle");
	await page.evaluate(() => document.fonts.ready);

	const viewport = page.viewportSize();
	if (viewport) {
		// A screen reads from the top; a click lower down may have left it scrolled. The wheel goes
		// to whatever scrolls under the pointer - the page, or an open dialog's body.
		await page.mouse.move(viewport.width / 2, viewport.height / 2);
		await page.mouse.wheel(0, -10_000);
		await page.evaluate(() => new Promise(requestAnimationFrame));

		// Park the pointer in an empty corner, so no row is caught mid-hover.
		await page.mouse.move(viewport.width - 4, viewport.height - 4);
	}

	await page.screenshot({
		path: `${screenshotsDir}${name}.png`,
		animations: "disabled",
		caret: "hide",
		style: hideTransient,
		mask: options.mask,
	});
}
