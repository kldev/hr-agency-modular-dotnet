import { expect, test as setup } from "@playwright/test";
import { authFile } from "../../playwright.config";
import { demoUser } from "../support/accounts";
import { open } from "../support/navigation";

/*
 * Signs in once and hands the session to every other test. The tokens live in httpOnly cookies
 * set by the TanStack Start server, so the storage state is a file of live credentials - it sits
 * under playwright/.auth, which git ignores.
 */
setup("sign in as the demo user", async ({ page }) => {
	await open(page, "/login");

	await page.getByLabel("Email address").fill(demoUser.email);
	await page.getByLabel("Password", { exact: true }).fill(demoUser.password);
	await page.getByRole("button", { name: "Sign in" }).click();

	await expect(
		page,
		"Sign-in failed - is the API up and seeded (GET /api/development/seed)?",
	).toHaveURL(/\/app\/dashboard/);

	// The panel's theme is a per-browser choice (`uiStore`, dark unless changed). Pinned here so
	// every screenshot is taken in the same one; E2E_THEME=light for the other.
	await page.evaluate(
		(theme) => localStorage.setItem("theme", theme),
		process.env.E2E_THEME ?? "dark",
	);

	await page.context().storageState({ path: authFile });
});
