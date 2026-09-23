import { expect, type Locator, type Page, test } from "@playwright/test";
import { open } from "../support/navigation";
import { docShot } from "../support/screenshots";

/*
 * A drop only opens the drawer: nothing is saved here, so the seed stays as it was and the test
 * can run any number of times.
 */

/** dnd-kit's mouse sensor starts after a few pixels, so the pointer has to travel in steps. */
async function drag(page: Page, card: Locator, target: Locator) {
	const from = await card.boundingBox();
	const to = await target.boundingBox();

	if (!from || !to) throw new Error("card or column is not on screen");

	await page.mouse.move(from.x + 20, from.y + 20);
	await page.mouse.down();
	await page.mouse.move(from.x + 40, from.y + 40, { steps: 5 });
	await page.mouse.move(to.x + to.width / 2, to.y + 80, { steps: 15 });
	await page.mouse.up();
}

function column(page: Page, name: string) {
	return page.getByRole("region", { name, exact: true });
}

test("a worker dropped on the next stage opens the status drawer with it preselected", async ({
	page,
}) => {
	await open(page, "/app/workers?view=kanban");

	const recruitment = column(page, "Recruitment");
	const card = recruitment.locator("article.kanban-card").first();
	await expect(card).toBeVisible();

	await docShot(page, "workers-kanban");

	await drag(page, card, column(page, "Contract preparation"));

	const drawer = page.getByRole("dialog", { name: "Change status" });
	await expect(drawer).toBeVisible();
	await expect(drawer.getByLabel("New status")).toHaveValue("ContractPreparation");
	await expect(drawer.getByLabel("Reason")).toBeVisible();

	await docShot(page, "workers-kanban-drop");
});

test("the abroad desk is the same board over the other half of the register", async ({ page }) => {
	await open(page, "/app/workers-abroad?view=kanban");

	await expect(page.getByRole("heading", { name: "Workers abroad", level: 1 })).toBeVisible();
	await expect(column(page, "Employed").locator("article.kanban-card").first()).toBeVisible();

	await docShot(page, "workers-abroad-kanban");
});

test("an application dropped on a stage it cannot reach opens nothing", async ({ page }) => {
	await open(page, "/app/applications?view=kanban");

	const card = column(page, "Applied").locator("article.kanban-card").first();
	await expect(card).toBeVisible();

	await docShot(page, "applications-kanban");

	await drag(page, card, column(page, "Offer"));
	await expect(page.getByRole("dialog")).toHaveCount(0);

	await drag(page, card, column(page, "Rejected"));

	const drawer = page.getByRole("dialog", { name: "Change application status" });
	await expect(drawer).toBeVisible();
	await expect(drawer.locator("select")).toHaveValue("Rejected");
	await expect(drawer.getByLabel("Note")).toBeVisible();

	await docShot(page, "applications-kanban-drop");
});
