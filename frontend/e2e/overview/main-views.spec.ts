import { expect, test } from "@playwright/test";
import { open } from "../support/navigation";
import { docShot } from "../support/screenshots";

/*
 * The panel's main lists as the seed fills them (the dashboard is still an empty page) - one screenshot each, and a smoke check that the
 * page renders its table with data rather than an empty state or an error.
 */
const views = [
	{ name: "job-postings", url: "/app/jobs" },
	{ name: "candidates", url: "/app/candidates" },
	{ name: "applications", url: "/app/applications" },
	{ name: "companies", url: "/app/companies" },
	{ name: "projects", url: "/app/projects" },
	{ name: "workers", url: "/app/workers" },
] as const;

for (const view of views) {
	test(`shows the ${view.name} view`, async ({ page }) => {
		await open(page, view.url);

		await expect(page.getByRole("heading", { level: 1 })).toBeVisible();
		await expect(page.getByRole("table").getByRole("row").nth(1)).toBeVisible();

		await docShot(page, view.name);
	});
}
