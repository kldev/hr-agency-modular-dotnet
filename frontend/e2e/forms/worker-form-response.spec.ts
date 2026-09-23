import { expect, type Page, test } from "@playwright/test";
import { open } from "../support/navigation";
import { docShot } from "../support/screenshots";
import { continueTo, expectStep, fillDate, summaryItem } from "../support/ui";

/*
 * Filling forms in for a worker, against the seeded forms: a consent nobody has given yet, a
 * questionnaire somebody left half way, and a submitted consent corrected with a reason. The seed
 * hands responses out in the order of the register, by surname - Bondarenko, Dubois, Ionescu and
 * Kovalenko have consents; Dubois stopped on the tax page; Petrov has nothing.
 */
async function openWorkerForms(page: Page, surname: string, name: string) {
	// The register is split by where people work now; whichever half they are on, the page is the same.
	await open(page, `/app/workers?search=${surname}`);
	const link = page.getByRole("link", { name }).first();

	if (!(await link.isVisible({ timeout: 3_000 }).catch(() => false))) {
		await open(page, `/app/workers-abroad?search=${surname}`);
	}

	await page.getByRole("link", { name }).first().click();
	await page.getByRole("tab", { name: "Forms" }).click();
}

test("gives a consent on a one page form", async ({ page }) => {
	await openWorkerForms(page, "Petrov", "Ivan Petrov");

	await page.getByRole("button", { name: "Add" }).click();
	const drawer = page.getByRole("dialog", { name: "Fill in a form" });
	await drawer.getByText("GDPR consent", { exact: true }).click();
	await drawer.getByRole("button", { name: "Save changes" }).click();

	const dialog = page.getByRole("dialog", { name: "GDPR consent" });
	await expect(dialog).toBeVisible();

	await dialog.getByRole("button", { name: "Submit" }).click();
	await expect(dialog.getByText("This box has to be ticked.")).toBeVisible();

	await dialog.getByLabel(/I consent to the processing/).check();
	await fillDate(dialog, "Date of consent *", "23.09.2026");

	await docShot(page, "worker-form-consent");

	await dialog.getByRole("button", { name: "Submit" }).click();
	await expect(dialog).toBeHidden();

	const row = page.getByRole("row").filter({ hasText: "GDPR consent" });
	await expect(row.getByText("Submitted")).toBeVisible();
});

test("finishes a questionnaire somebody left on the tax page", async ({ page }) => {
	await openWorkerForms(page, "Dubois", "Lucas Dubois");

	await page
		.getByRole("row")
		.filter({ hasText: "Personal questionnaire" })
		.getByRole("button", { name: "Continue" })
		.click();

	const dialog = page.getByRole("dialog", { name: "Personal questionnaire" });
	await expectStep(dialog, "Personal data");

	// Pre-filled from the worker's file when the response was started.
	await expect(dialog.getByLabel("First name")).toHaveValue("Lucas");

	await dialog.getByLabel("PESEL").fill("123");
	await dialog.getByRole("button", { name: "Continue" }).click();
	await expect(dialog.getByText("Enter a valid PESEL number (11 digits).")).toBeVisible();
	await dialog.getByLabel("PESEL").fill("");

	await continueTo(dialog, "Contact");
	await continueTo(dialog, "Tax");
	await expect(dialog.getByLabel("Tax office *")).toHaveValue("Urząd Skarbowy Kraków-Podgórze");

	await dialog.getByRole("button", { name: "Continue" }).click();
	await expect(dialog.getByText("This field is required.").first()).toBeVisible();

	await dialog.getByLabel("Country of tax residence *").selectOption("FR");
	await dialog.getByText("No declaration", { exact: true }).click();

	await docShot(page, "worker-form-wizard");

	await continueTo(dialog, "Payment");
	await dialog.getByLabel(/I declare that the data above is true/).check();
	await continueTo(dialog, "Review");
	await expect(summaryItem(dialog, "Country of tax residence")).toContainText("France");

	await docShot(page, "worker-form-review");

	await dialog.getByRole("button", { name: "Submit" }).click();
	await expect(dialog).toBeHidden();
	await expect(
		page.getByRole("row").filter({ hasText: "Personal questionnaire" }).getByText("Submitted"),
	).toBeVisible();
});

test("corrects a submitted consent with a reason", async ({ page }) => {
	await openWorkerForms(page, "Bondarenko", "Mykola Bondarenko");

	await page
		.getByRole("row")
		.filter({ hasText: "GDPR consent" })
		.getByRole("button", { name: "Open" })
		.click();

	const dialog = page.getByRole("dialog", { name: "GDPR consent" });
	await expect(summaryItem(dialog, "I agree to receive job offers by e-mail.")).toContainText(
		"Yes",
	);

	await docShot(page, "worker-form-submitted");

	await dialog.getByRole("button", { name: "Correct" }).click();
	await dialog.getByLabel(/I agree to receive job offers/).uncheck();
	await dialog
		.getByLabel("Why is it being corrected?")
		.fill("Withdrew the marketing consent by e-mail.");
	await dialog.getByRole("button", { name: "Save correction" }).click();
	await expect(dialog).toBeHidden();

	await page
		.getByRole("row")
		.filter({ hasText: "GDPR consent" })
		.getByRole("button", { name: "Open" })
		.click();
	await expect(dialog.getByText(/rev\. 1|Revision 1/)).toBeVisible();
	await expect(dialog.getByText(/Withdrew the marketing consent/)).toBeVisible();
});
