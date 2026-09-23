import { expect, type Page, test } from "@playwright/test";
import { open } from "../support/navigation";
import { docShot } from "../support/screenshots";
import { pickSuggestion } from "../support/ui";

/*
 * A reorganisation on top of the seeded chart: the recruitment department grows two sections and
 * one of its people moves over to lead the first of them. A person belongs to one unit, so they
 * are taken out of the department before they can be put anywhere else.
 */
const person = "Adrian Jimbo";

/** A unit in the chart; its button reads "<name> <kind> · <head>". */
function unit(page: Page, name: string) {
	return page.getByRole("button", { name: new RegExp(`^${name} (Board|Department|Section)`) });
}

async function addUnitUnder(page: Page, parent: string, name: string) {
	await unit(page, parent).click();
	await page.getByRole("button", { name: `Actions for ${parent}` }).click();
	await page.getByRole("menuitem", { name: "Add unit under this one" }).click();

	const drawer = page.getByRole("dialog", { name: `New unit under ${parent}` });
	await drawer.getByLabel("Name").fill(name);
	await expect(drawer.getByLabel("Kind")).toHaveValue("Section");
	await drawer.getByRole("button", { name: "Save changes" }).click();
	await expect(drawer).toBeHidden();
	await expect(unit(page, name)).toBeVisible();
}

test("reorganises a department in the org chart", async ({ page }) => {
	await open(page, "/app/org-structure");
	await expect(page.getByRole("heading", { name: "Chart" })).toBeVisible();

	await addUnitUnder(page, "Recruitment", "Talent Acquisition");
	await addUnitUnder(page, "Recruitment", "Sourcing");

	// Out of the department first - the chart refuses a second unit for the same person.
	await unit(page, "Recruitment").click();
	await page.getByRole("button", { name: `Actions for ${person}` }).click();
	await page.getByRole("menuitem", { name: "Take out of this unit" }).click();
	await page
		.getByRole("dialog", { name: "Take out of the unit" })
		.getByRole("button", { name: "Take out" })
		.click();
	await expect(page.getByRole("button", { name: `Actions for ${person}` })).toBeHidden();

	await unit(page, "Talent Acquisition").click();
	await expect(page.getByText("Nobody here yet.")).toBeVisible();

	await page.getByRole("button", { name: "Add person" }).click();
	const add = page.getByRole("dialog", { name: "Add to Talent Acquisition" });
	await pickSuggestion(add, "Person", "Adrian", new RegExp(person));
	await add.getByLabel("Title (optional)").fill("Talent Acquisition Lead");
	await add.getByRole("button", { name: "Save changes" }).click();
	await expect(add).toBeHidden();

	await page.getByRole("button", { name: "Assign" }).click();
	const head = page.getByRole("dialog", { name: "Head of Talent Acquisition" });
	await head.getByLabel("Head").selectOption({ label: person });
	await head.getByRole("button", { name: "Save changes" }).click();
	await expect(head).toBeHidden();

	// Saved, not just drawn: a fresh page reads the chart back from the server.
	await page.reload();
	await page.waitForLoadState("networkidle");
	await expect(unit(page, "Talent Acquisition")).toContainText(person);
	await expect(unit(page, "Sourcing")).toContainText("led from above");

	await unit(page, "Talent Acquisition").click();
	await expect(page.getByText("Talent Acquisition Lead")).toBeVisible();

	// Folded so the department that changed fits on the screen next to its panel.
	for (const branch of ["Operations", "Payroll"]) {
		await page.getByRole("button", { name: `Collapse ${branch}` }).click();
	}

	await docShot(page, "organization-structure");
});
