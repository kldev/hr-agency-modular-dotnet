import { expect, test } from "@playwright/test";
import { open } from "../support/navigation";
import { docShot } from "../support/screenshots";

/*
 * Read-only views over the seed's delivery data, which is written by hand to show the awkward
 * cases. A Polish project owes nothing to a host country, so the compliance checklist is shown on
 * the seed's German temporary agency work project - the variant with the most duties.
 */

test("shows the compliance checklist of a hired-out project", async ({ page }) => {
	const project = "Hamburg logistics platform";

	await open(page, `/app/projects?search=${encodeURIComponent(project)}`);
	await page.getByRole("table").getByRole("link", { name: project }).click();

	await expect(page.getByRole("heading", { name: project })).toBeVisible();
	await page.getByRole("tab", { name: /^Compliance/ }).click();

	const checklist = page.getByRole("tabpanel");
	await expect(checklist.getByRole("button", { name: "Record" }).first()).toBeVisible();

	await docShot(page, "project-compliance");
});

test("lists the assignments register", async ({ page }) => {
	await open(page, "/app/assignments");

	// Hand-written cast of the seed: postings to Germany and Belgium, among others.
	await expect(
		page.getByRole("table").getByText("Munich core banking rollout").first(),
	).toBeVisible();
	await expect(
		page.getByRole("table").getByText("Brussels payments integration").first(),
	).toBeVisible();

	await docShot(page, "assignments");
});
