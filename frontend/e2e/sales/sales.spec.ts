import { expect, test } from "@playwright/test";
import { createDemoOpportunity, ensureDemoClient, reloadUntilVisible } from "../support/api";
import { open } from "../support/navigation";
import { docShot } from "../support/screenshots";

/*
 * The sales pipeline. The board and the table are filled by `seed-sales`; the opportunity whose
 * details are shown is the demo one, so that page looks the same on every run.
 */
const title = "Connected-car platform team";

test("shows the sales pipeline and an opportunity", async ({ page }) => {
	const companyId = await ensureDemoClient(page);
	const opportunityId = await createDemoOpportunity(page, companyId, title);

	await open(page, "/app/sales");
	await expect(page.getByRole("heading", { name: "Sales", level: 1 })).toBeVisible();
	await expect(page.getByRole("table").getByRole("row").nth(1)).toBeVisible();

	await docShot(page, "sales");

	await page.getByRole("button", { name: "Kanban view" }).click();
	await expect(page).toHaveURL(/view=kanban/);
	await expect(page.getByRole("link", { name: title })).toBeVisible();

	await docShot(page, "sales-kanban");

	await open(page, `/app/sales/opportunities/${opportunityId}`);
	await reloadUntilVisible(page, () =>
		expect(page.getByText("Sent the proposal", { exact: false })).toBeVisible({ timeout: 2_000 }),
	);
	await expect(page.getByRole("heading", { name: title })).toBeVisible();

	await docShot(page, "sales-opportunity");
});
