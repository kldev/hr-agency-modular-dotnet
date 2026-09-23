import { expect, test } from "@playwright/test";
import { demoOwner } from "../support/accounts";
import { moveApplicationsThroughFunnel, waitForReportedHires } from "../support/api";
import { open } from "../support/navigation";
import { docShot } from "../support/screenshots";

/*
 * The two reports, drawn by the reports service from its own tables. Applications are moved along
 * first, so the funnel reaches offers and hires instead of stopping where the seed leaves it.
 */

test("shows the recruitment dashboard", async ({ page }) => {
	await moveApplicationsThroughFunnel(page);
	await waitForReportedHires(page, 4);

	await open(page, "/app/dashboard?range=6");

	await expect(page.getByRole("heading", { name: "Funnel" })).toBeVisible();
	await expect(page.getByRole("heading", { name: "Month by month" })).toBeVisible();
	await expect(page.getByRole("img", { name: "Recruitment funnel by stage" })).toBeVisible();
	await expect(page.getByRole("button", { name: "6 months" })).toHaveAttribute(
		"aria-pressed",
		"true",
	);
	await expect(page.getByRole("link", { name: "Export to Excel" })).toHaveAttribute(
		"href",
		/\/api\/reports\/recruitment\/export\?from=\d{4}-\d{2}&to=\d{4}-\d{2}/,
	);

	await docShot(page, "dashboard");
});

test.describe("platform owner", () => {
	// The owner signs in on a page of their own, so this test starts signed out.
	test.use({ storageState: { cookies: [], origins: [] } });

	test("shows every organization side by side", async ({ page }) => {
		await open(page, "/owner");
		await page.getByLabel("Email address").fill(demoOwner.email);
		await page.getByPlaceholder("Enter your password").fill(demoOwner.password);
		await page.getByRole("button", { name: /sign in/i }).click();
		await page.waitForURL(/\/admin/);

		await open(page, "/admin/reports");

		await expect(page.getByRole("heading", { name: "Busiest organizations" })).toBeVisible();

		const table = page.getByRole("table");
		for (const organization of ["Hr Agency", "Flex Jobs", "Tech Jobs"]) {
			await expect(table.getByText(organization, { exact: true })).toBeVisible();
		}

		await docShot(page, "platform-reports");
	});
});
