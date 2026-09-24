import { expect, type Page, test } from "@playwright/test";
import { reloadUntilVisible } from "../support/api";
import { open } from "../support/navigation";
import { docShot } from "../support/screenshots";
import { pickSuggestion } from "../support/ui";

/*
 * The salesperson's workspace (plan 029). The four named clients, their deals, projects and a
 * to-do list come from the seed's workspace scenario; the demo account is one of the two people
 * the seed gives tasks to. Tasks this spec creates are dated around "now", so it means the same
 * on any day it runs.
 */

type CompanyRow = { id: string; name: string };

async function companyId(page: Page, name: string): Promise<string> {
	const response = await page.request.get("/api/companies", {
		params: { search: name, pageSize: 5 },
	});
	expect(response.ok(), `GET companies: ${response.status()}`).toBeTruthy();

	const { content } = (await response.json()) as { content: CompanyRow[] };
	const company = content.find((row) => row.name === name);

	expect(company, `the seed has no "${name}"`).toBeDefined();
	return (company as CompanyRow).id;
}

async function createTask(page: Page, company: string, title: string, dueAt: Date) {
	const response = await page.request.post("/api/tasks", {
		data: { companyId: company, title, dueAt: dueAt.toISOString(), priority: "High" },
	});
	expect(response.ok(), `POST task: ${response.status()} ${await response.text()}`).toBeTruthy();
}

const tasks = (page: Page, section: "Active tasks" | "Completed tasks") =>
	page.getByRole("list", { name: section });

test("shows a company's activities, opportunities and projects and switches company", async ({
	page,
}) => {
	const acme = await companyId(page, "ACME Sp. z o.o.");

	await open(page, `/app/sales-workspace?companyId=${acme}`);

	await expect(page.getByRole("heading", { name: "ACME Sp. z o.o." })).toBeVisible();
	await expect(page.getByText("5260001001")).toBeVisible();
	await expect(
		page.getByRole("table").getByText("Discussed candidate requirements with Anna Kowalska."),
	).toBeVisible();

	await docShot(page, "sales-workspace");

	await page.getByRole("tab", { name: "Opportunities" }).click();
	await expect(page).toHaveURL(/tab=opportunities/);
	await expect(page.getByRole("link", { name: "Recruitment Q4" })).toBeVisible();

	await page.getByRole("tab", { name: "Projects" }).click();
	await expect(page).toHaveURL(/tab=projects/);
	const warehouse = page.getByRole("row").filter({ hasText: "Warehouse 2026" });
	await expect(warehouse.getByRole("link", { name: "Warehouse Workers" })).toBeVisible();

	await docShot(page, "sales-workspace-projects");

	await page.getByRole("button", { name: "Change company" }).click();
	await pickSuggestion(page, "Company", "Auto Parts", "Auto Parts Polska");

	const autoParts = await companyId(page, "Auto Parts Polska");
	await expect(page).toHaveURL(new RegExp(`companyId=${autoParts}`));
	await expect(page.getByRole("heading", { name: "Auto Parts Polska" })).toBeVisible();

	await page.getByRole("tab", { name: "Opportunities" }).click();
	await expect(page.getByRole("link", { name: "Production Workers", exact: true })).toBeVisible();
	await expect(page.getByRole("link", { name: "Recruitment Q4" })).toBeHidden();
});

test("cuts the task list by day, week and month", async ({ page }) => {
	const acme = await companyId(page, "ACME Sp. z o.o.");
	const later = `Prepare the quarterly review ${Date.now()}`;
	const due = new Date(Date.now() + 20 * 24 * 60 * 60 * 1000);
	await createTask(page, acme, later, due);

	await open(page, `/app/sales-workspace?companyId=${acme}&range=day`);
	await expect(tasks(page, "Active tasks").getByText("Follow up with purchasing")).toBeVisible();
	await expect(tasks(page, "Active tasks").getByText(later)).toBeHidden();

	// Twenty days ahead is in this month only when the month has twenty days left.
	const now = new Date();
	const inThisMonth = due < new Date(now.getFullYear(), now.getMonth() + 1, 1);

	await page.getByRole("button", { name: "Month" }).click();
	await expect(page).toHaveURL(/range=month/);
	await reloadUntilVisible(page, async () => {
		await expect(tasks(page, "Active tasks").getByText(later)).toHaveCount(inThisMonth ? 1 : 0);
		await expect(tasks(page, "Active tasks").getByText("Follow up with purchasing")).toBeVisible();
	});
});

test("ticks a task off with one click and takes it back", async ({ page }) => {
	const acme = await companyId(page, "ACME Sp. z o.o.");
	const title = `Call the warehouse manager ${Date.now()}`;
	// An hour overdue: open and overdue tasks are on every range, so the default week shows it.
	await createTask(page, acme, title, new Date(Date.now() - 60 * 60 * 1000));

	await open(page, `/app/sales-workspace?companyId=${acme}`);
	await reloadUntilVisible(page, () =>
		expect(tasks(page, "Active tasks").getByText(title)).toBeVisible({ timeout: 2_000 }),
	);

	await page.getByRole("checkbox", { name: `Mark ${title} as done` }).click();

	await expect(tasks(page, "Completed tasks").getByText(title)).toBeVisible();
	await expect(tasks(page, "Active tasks").getByText(title)).toBeHidden();

	await docShot(page, "sales-workspace-tasks");

	// Not a local illusion: after a reload the API still has it done.
	await reloadUntilVisible(page, () =>
		expect(tasks(page, "Completed tasks").getByText(title)).toBeVisible({ timeout: 2_000 }),
	);

	await page.getByRole("checkbox", { name: `Reopen ${title}` }).click();
	await expect(tasks(page, "Active tasks").getByText(title)).toBeVisible();
});

test("adds a task for the company on screen", async ({ page }) => {
	const acme = await companyId(page, "ACME Sp. z o.o.");
	const title = `Send the signed contract ${Date.now()}`;

	await open(page, `/app/sales-workspace?companyId=${acme}`);
	await page.getByRole("button", { name: "Add task" }).click();

	const drawer = page.getByRole("dialog", { name: "New task" });
	await expect(drawer.getByRole("combobox", { name: "Company" })).toHaveValue("ACME Sp. z o.o.");
	await drawer.getByLabel("Opportunity").selectOption({ label: "Recruitment Q4" });
	await drawer.getByLabel("Title").fill(title);
	await drawer.getByRole("button", { name: "Save changes" }).click();

	await expect(drawer).toBeHidden();
	await reloadUntilVisible(page, () =>
		expect(tasks(page, "Active tasks").getByText(title)).toBeVisible({ timeout: 2_000 }),
	);
	await expect(
		tasks(page, "Active tasks").getByRole("listitem").filter({ hasText: title }),
	).toContainText("Recruitment Q4");
});
