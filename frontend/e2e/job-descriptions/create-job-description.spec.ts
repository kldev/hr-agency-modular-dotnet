import { expect, test } from "@playwright/test";
import { demoClient, ensureDemoClient, reloadUntilVisible } from "../support/api";
import { open } from "../support/navigation";
import { docShot } from "../support/screenshots";
import { continueTo, expectStep, pickSuggestion, summaryItem } from "../support/ui";

const title = "Senior C# Developer";

test("creates a job description through the wizard", async ({ page }) => {
	await ensureDemoClient(page);

	await open(page, "/app/job-descriptions");

	// The wizard opens in a tab of its own, so a half written description survives the list.
	const [wizard] = await Promise.all([
		page.waitForEvent("popup"),
		page.getByRole("button", { name: "Add job description" }).click(),
	]);
	await wizard.waitForLoadState("networkidle");

	// Position
	await expectStep(wizard, "Job basics");
	await wizard.getByRole("button", { name: "Continue" }).click();
	await expect(wizard.getByText("Company is required")).toBeVisible();
	await expect(wizard.getByText("Title is required")).toBeVisible();

	await pickSuggestion(wizard, "Company", "ACME", demoClient.name);
	await pickSuggestion(wizard, "Recruiter", "Agnieszka", /Agnieszka Mazur/);
	await wizard.getByLabel("Job title").fill(title);
	await wizard
		.getByLabel("Short summary")
		.fill(
			"Join the platform team behind ACME's connected-car services and own .NET backends end to end.",
		);

	await docShot(wizard, "job-description-wizard");

	// Content
	await wizard.getByRole("button", { name: "Continue" }).click();
	await wizard
		.getByLabel("Description")
		.fill(
			"We are looking for a senior engineer to design and run the services that collect telemetry from 400 000 vehicles. You will work in a cross-functional team of eight, with real ownership of architecture decisions.",
		);
	const responsibilities = wizard.getByRole("group", { name: "Responsibilities" });
	await responsibilities
		.getByRole("textbox", { name: "Responsibilities 1", exact: true })
		.fill("Design and build .NET microservices");
	await responsibilities.getByRole("button", { name: "Add item" }).click();
	await responsibilities
		.getByRole("textbox", { name: "Responsibilities 2", exact: true })
		.fill("Mentor mid-level developers");

	// Requirements
	await wizard.getByRole("button", { name: "Continue" }).click();
	const requirements = wizard.getByRole("group", { name: "Requirements" });
	await requirements
		.getByRole("textbox", { name: "Requirements 1", exact: true })
		.fill("5+ years of commercial experience with C#");
	await requirements.getByRole("button", { name: "Add item" }).click();
	await requirements
		.getByRole("textbox", { name: "Requirements 2", exact: true })
		.fill("Solid knowledge of PostgreSQL");

	const skills = wizard.getByRole("group", { name: "Skills" });
	await skills.getByRole("textbox", { name: "Skills 1", exact: true }).fill("C#");
	for (const [index, skill] of ["ASP.NET Core", "PostgreSQL", "Docker"].entries()) {
		await skills.getByRole("button", { name: "Add item" }).click();
		await skills.getByRole("textbox", { name: `Skills ${index + 2}`, exact: true }).fill(skill);
	}

	// Employment
	await wizard.getByRole("button", { name: "Continue" }).click();
	await wizard.getByRole("textbox", { name: "Location" }).fill("Opole");
	await wizard.getByLabel("Country").selectOption("PL");
	await wizard.getByRole("radio", { name: /^Hybrid/ }).check();
	await wizard.getByRole("radio", { name: /^Full time/ }).check();
	await wizard.getByLabel("Minimum").fill("22000");
	await wizard.getByLabel("Maximum").fill("18000");
	await wizard.getByLabel("Currency").selectOption("PLN");

	await wizard.getByRole("button", { name: "Continue" }).click();
	await expect(wizard.getByText("Maximum salary cannot be lower than the minimum")).toBeVisible();
	await wizard.getByLabel("Maximum").fill("28000");

	// Review
	await continueTo(wizard, "Review");
	await expect(summaryItem(wizard, "Job title")).toContainText(title);
	await expect(summaryItem(wizard, "Location")).toContainText("Opole");
	await expect(summaryItem(wizard, "Work mode")).toContainText("Hybrid");
	await expect(wizard.getByText("ASP.NET Core")).toBeVisible();

	await docShot(wizard, "job-description-review");

	await wizard.getByRole("button", { name: "Create job description" }).click();
	await expect(wizard).toHaveURL(/\/app\/job-descriptions(\?|$)/);

	// Newest first, so a leftover from an earlier run never shadows the one just created.
	const row = wizard
		.getByRole("row")
		.filter({ hasText: title })
		.filter({ hasText: demoClient.name })
		.first();
	await reloadUntilVisible(wizard, () => expect(row).toBeVisible({ timeout: 2_000 }));

	await row.getByRole("button", { name: "More actions" }).click();
	const [details] = await Promise.all([
		wizard.waitForEvent("popup"),
		wizard.getByRole("menuitem", { name: "Open details" }).click(),
	]);
	await details.waitForLoadState("networkidle");

	await expect(details).toHaveURL(/\/app\/job-descriptions\/[0-9a-f-]{36}/);
	await expect(details.getByRole("heading", { name: title })).toBeVisible();

	await docShot(details, "job-description-created");
});
