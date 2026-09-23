import { expect, test } from "@playwright/test";
import {
	type Applicant,
	applyToJobPost,
	findPublishedJobPost,
	reloadUntilVisible,
} from "../support/api";
import { open } from "../support/navigation";
import { docShot } from "../support/screenshots";
import { continueTo, expectStep, fillDate, summaryItem } from "../support/ui";

/*
 * The hand-over from recruitment to delivery: somebody who applied through us is taken onto the
 * workers' register. The application is the fixture; the register wizard is the flow.
 * A fictional person - the register is the place people's documents live.
 */
const applicant: Applicant = {
	firstName: "Olena",
	lastName: "Marchenko",
	email: "olena.marchenko@example.com",
	phoneNumber: "+48 600 100 200",
};

test("registers an applicant as a worker", async ({ page }) => {
	const post = await findPublishedJobPost(page, "C# Developer");
	await applyToJobPost(page, post.id, applicant);

	await open(page, `/app/applications?search=${applicant.lastName}`);

	const row = page
		.getByRole("row")
		.filter({ hasText: `${applicant.firstName} ${applicant.lastName}` });
	await reloadUntilVisible(page, () => expect(row).toBeVisible({ timeout: 2_000 }));

	await row.getByRole("button", { name: "More actions" }).click();
	await page.getByRole("menuitem", { name: "Register as worker" }).click();

	const wizard = page.getByRole("dialog", { name: "Register worker" });
	await expect(wizard).toBeVisible();

	// Identity - name and contact come over from the application, the rest is the passport's.
	await expectStep(wizard, "Identity");
	await expect(wizard.getByLabel("First name")).toHaveValue(applicant.firstName);
	await expect(wizard.getByLabel("Last name")).toHaveValue(applicant.lastName);

	await wizard.getByRole("button", { name: "Continue" }).click();
	await expect(wizard.getByText("Date of birth is required")).toBeVisible();
	await expect(wizard.getByText("Citizenship is required")).toBeVisible();

	await fillDate(wizard, "Date of birth", "14.05.1994");
	await wizard.getByLabel("Citizenship").selectOption("UA");
	await expect(wizard.getByText(/goes through the legalisation stage/)).toBeVisible();

	await docShot(page, "worker-wizard");

	await continueTo(wizard, "Identity document");
	await wizard.getByLabel("Kind").selectOption({ label: "Passport" });
	await wizard.getByLabel("Document number").fill("FX482913");
	await wizard.getByLabel("Issued by").selectOption("UA");
	await fillDate(wizard, "Valid until", "30.06.2031");

	await continueTo(wizard, "Contact");
	await expect(wizard.getByLabel("Email")).toHaveValue(applicant.email);
	await wizard.getByLabel("Street").fill("ul. Opolska");
	await wizard.getByLabel("Building number").fill("12");
	await wizard.getByLabel("Postal code").fill("45-001");
	await wizard.getByLabel("City").fill("Opole");
	await wizard.getByLabel("Country").selectOption("PL");

	await continueTo(wizard, "Review");
	await expect(summaryItem(wizard, "Name")).toContainText("Olena Marchenko");
	await expect(summaryItem(wizard, "Citizenship")).toContainText("Ukraine");
	await expect(summaryItem(wizard, "Number")).toContainText("FX482913");
	await expect(summaryItem(wizard, "Address")).toContainText("Opole");

	await docShot(page, "worker-wizard-review");

	await wizard.getByRole("button", { name: "Register worker" }).click();

	await expect(page).toHaveURL(/\/app\/workers\/[0-9a-f-]{36}/);
	await expect(page.getByRole("heading", { name: "Olena Marchenko" })).toBeVisible();

	await docShot(page, "worker-created");
});
