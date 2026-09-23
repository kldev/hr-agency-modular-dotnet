import { expect, type Page, test } from "@playwright/test";
import { demoClient, ensureDemoClient } from "../support/api";
import { attachDocument } from "../support/documents";
import { open } from "../support/navigation";
import { docShot } from "../support/screenshots";
import { continueTo, expectStep, fillDate, pickSuggestion, summaryItem } from "../support/ui";

/*
 * The main business flow of the delivery side: a client, a project for it, a signed contract and
 * somebody responsible on their side - which is exactly what a project needs to go live - the
 * first role on it, and the first person posted onto that role.
 */
const projectName = "Senior Developers Recruitment";

/** One of the seed's hand-written workers with no posting yet. */
const worker = "Sofia Ionescu";

async function openTab(page: Page, name: string) {
	await page.getByRole("tab", { name: new RegExp(`^${name}`) }).click();
	await expect(page.getByRole("tabpanel")).toBeVisible();
}

test("creates a project, takes it live and posts a worker onto it", async ({ page }) => {
	await ensureDemoClient(page);

	await open(page, "/app/projects");
	await page.getByRole("button", { name: "Add project" }).click();

	const wizard = page.getByRole("dialog", { name: "New project" });
	await expect(wizard).toBeVisible();

	// Parties
	await expectStep(wizard, "Client");
	await wizard.getByRole("button", { name: "Continue" }).click();
	await expect(wizard.getByText("Pick a client")).toBeVisible();

	await pickSuggestion(wizard, "Client", "ACME", demoClient.name);
	await wizard.getByLabel("Our company").selectOption({ index: 1 });

	// Basics
	await continueTo(wizard, "Basics");
	await wizard.getByLabel("Name", { exact: true }).fill(projectName);
	await wizard
		.getByLabel("Description")
		.fill(
			"Building ACME's in-house team for the connected-car platform: three senior .NET engineers, a frontend developer and a DevOps engineer, delivered as an outsourced team.",
		);
	await wizard.getByRole("radio", { name: /^Outsourcing/ }).check();

	await docShot(page, "project-wizard");

	// Place of work and period
	await continueTo(wizard, "Place of work and period");
	await wizard.getByLabel("Street").fill("ul. Fabryczna");
	await wizard.getByLabel("Building number").fill("8");
	await wizard.getByLabel("Postal code").fill("45-125");
	await wizard.getByLabel("City").fill("Opole");
	await wizard.getByLabel("Country").selectOption("PL");
	await fillDate(wizard, "Starts on", "01.10.2026");
	await fillDate(wizard, "Ends on", "30.09.2027");

	// Team - one of the seed's recruitment teams runs it on our side
	await continueTo(wizard, "Team");
	await pickSuggestion(wizard, "Team", "Byte", "Byte Wranglers");

	// Review
	await continueTo(wizard, "Review");
	await expect(summaryItem(wizard, "Client")).toContainText(demoClient.name);
	await expect(summaryItem(wizard, "Name")).toContainText(projectName);
	await expect(summaryItem(wizard, "Engagement")).toContainText("Outsourcing");
	await expect(summaryItem(wizard, "Place of work")).toContainText("Opole");

	await wizard.getByRole("button", { name: "Create project" }).click();

	await expect(page).toHaveURL(/\/app\/projects\/[0-9a-f-]{36}/);
	await expect(page.getByRole("heading", { name: projectName })).toBeVisible();

	// A signed contract and a responsible contact - two of the three go-live gates. The third, a
	// complete client profile, the fixture already took care of.
	await openTab(page, "Contract & contacts");

	await page.getByRole("button", { name: "Record contract" }).click();
	const contract = page.getByRole("dialog", { name: "Record contract" });
	await contract.getByLabel("Contract number").fill("ACME/2026/014");
	await contract.getByLabel("Status").selectOption("Signed");
	await fillDate(contract, "Signed on", "15.09.2026");
	await fillDate(contract, "Valid from", "01.10.2026");
	await fillDate(contract, "Valid to", "30.09.2027");
	await contract.getByRole("button", { name: "Save changes" }).click();
	await expect(contract).toBeHidden();
	await expect(page.getByText("ACME/2026/014")).toBeVisible();

	await page
		.getByRole("group", { name: "Responsible" })
		.getByRole("button", { name: "Assign" })
		.click();
	const contact = page.getByRole("dialog", { name: "Assign responsible" });
	await pickSuggestion(contact, "Copy from the client's contacts", "Anna", /Anna Kowalska/);
	await expect(contact.getByLabel("First name")).toHaveValue("Anna");
	await contact.getByRole("button", { name: "Save changes" }).click();
	await expect(contact).toBeHidden();
	await expect(page.getByRole("group", { name: "Responsible" })).toContainText("Anna Kowalska");

	// Go live
	await page.getByRole("button", { name: "More actions" }).click();
	await page.getByRole("menuitem", { name: "Change status" }).click();
	const status = page.getByRole("dialog", { name: "Change status" });
	await status.getByLabel("New status").selectOption("Active");
	await expect(status.getByText("The contract is signed")).toBeVisible();
	await status.getByRole("button", { name: "Save changes" }).click();
	await expect(status).toBeHidden();
	await expect(page.getByText("Active", { exact: true }).first()).toBeVisible();

	await openTab(page, "Contract & contacts");
	await docShot(page, "project-contract");

	// The first role on the project
	await openTab(page, "Positions");
	await page.getByRole("tabpanel").getByRole("button", { name: "Add", exact: true }).click();

	const position = page.getByRole("dialog", { name: "New position" });
	await expectStep(position, "Role");
	await position.getByLabel("Name", { exact: true }).fill("Senior C# Developer");
	await position.getByLabel("Target headcount").fill("3");
	await continueTo(position, "Work");
	await continueTo(position, "Contract");
	await position.getByRole("radio", { name: /^Self-employed \(B2B\)/ }).check();
	await continueTo(position, "Time and place");
	await continueTo(position, "Review");
	await position.getByRole("button", { name: "Open position" }).click();
	await expect(position).toBeHidden();
	await expect(page.getByRole("tabpanel").getByText("Senior C# Developer")).toBeVisible();

	await docShot(page, "project-positions");

	// The paperwork behind the engagement, kept in the file service.
	await openTab(page, "Documents");
	await attachDocument(page, page.getByRole("tabpanel"), {
		fileName: "acme-framework-agreement.pdf",
		title: "Framework agreement ACME/2026/014",
		lines: ["Client: ACME Automotive Sp. z o.o.", "Outsourced team, 12 months"],
		category: "Contract",
		documentDate: "15.09.2026",
		validUntil: "30.09.2027",
	});
	await attachDocument(page, page.getByRole("tabpanel"), {
		fileName: "acme-order-001.pdf",
		title: "Order 001 - Senior Developers Recruitment",
		lines: ["Five roles, start 01.10.2026"],
		category: "Annex",
		documentDate: "20.09.2026",
	});
	await attachDocument(page, page.getByRole("tabpanel"), {
		fileName: "liability-insurance-2026.pdf",
		title: "Professional liability insurance",
		lines: ["Policy covering the delivery team"],
		category: "Insurance",
		documentDate: "01.09.2026",
		validUntil: "31.08.2027",
	});

	await docShot(page, "project-documents");

	// Somebody from the workers' register onto that role. Opened from the project, so the wizard
	// already knows which one and skips the step that would ask.
	await openTab(page, "People");
	await page.getByRole("tabpanel").getByRole("button", { name: "Add", exact: true }).click();

	const plan = page.getByRole("dialog", { name: "Plan assignment" });
	await expectStep(plan, "Worker");
	await pickSuggestion(plan, "Worker", "Sofia", /Sofia Ionescu/);

	await continueTo(plan, "Terms");
	await plan.getByRole("radio", { name: /^Outsourcing/ }).check();
	await pickSuggestion(plan, "Position", "Senior", "Senior C# Developer");
	await fillDate(plan, "Starts on", "01.10.2026");
	await fillDate(plan, "Ends on", "31.03.2027");

	await continueTo(plan, "Review");
	await expect(summaryItem(plan, "Worker")).toContainText(worker);
	await expect(summaryItem(plan, "Project")).toContainText(projectName);
	await expect(summaryItem(plan, "Position")).toContainText("Senior C# Developer");

	await docShot(page, "assignment-wizard-review");

	await plan.getByRole("button", { name: "Plan assignment" }).click();
	await expect(plan).toBeHidden();

	const person = page.getByRole("tabpanel").getByRole("link", { name: worker });
	await expect(person).toBeVisible();
	await docShot(page, "project-people");

	await openTab(page, "Overview");
	await expect(page.getByText(demoClient.legalName).first()).toBeVisible();
	await expect(page.getByText("Byte Wranglers").first()).toBeVisible();

	await docShot(page, "project");

	// The posting itself: one person, one role, one period.
	await openTab(page, "People");
	await page.getByRole("tabpanel").getByRole("link", { name: worker }).click();

	await expect(page).toHaveURL(/\/app\/assignments\/[0-9a-f-]{36}/);
	await expect(page.getByRole("heading", { name: worker })).toBeVisible();
	await expect(page.getByText(projectName).first()).toBeVisible();

	await docShot(page, "assignment");
});
