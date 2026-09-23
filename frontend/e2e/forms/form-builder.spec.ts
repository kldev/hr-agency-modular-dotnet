import { expect, test } from "@playwright/test";
import { open } from "../support/navigation";
import { docShot } from "../support/screenshots";
import { continueTo, expectStep, summaryItem } from "../support/ui";

/*
 * An administrator builds a two page statement without a developer: a page of the form's own
 * questions and a system field from the catalogue, then a declaration on the second page. The
 * preview is the same renderer people fill it in with, so it is where the page validation is shown.
 */
test("builds, previews and publishes a two page form", async ({ page }) => {
	await open(page, "/app/forms");
	await expect(page.getByRole("link", { name: "GDPR consent" })).toBeVisible();

	await docShot(page, "forms-list");

	await page.getByRole("button", { name: "New form" }).click();

	const drawer = page.getByRole("dialog", { name: "New form" });
	await drawer.getByLabel("Name").fill("Company car statement");
	await expect(drawer.getByLabel("Code (never changes)")).toHaveValue("company-car-statement");
	await drawer.getByRole("button", { name: "Save changes" }).click();

	await expect(
		page.getByRole("heading", { name: "Company car statement", level: 1 }),
	).toBeVisible();

	// Page one: the form's own questions and the worker's first name from the catalogue.
	await page.getByRole("button", { name: "Add page" }).click();
	await page.getByLabel("Page title").fill("Vehicle");

	await page.getByRole("button", { name: "Add question" }).click();
	await page.getByLabel("Label *").fill("Registration number");
	await expect(page.getByLabel(/^Code \*/)).toHaveValue("companyCarStatement.registrationNumber");
	await page.getByLabel("Required", { exact: true }).check({ force: true });
	await page.getByLabel(/^Pattern/).fill("[A-Z]{2,3}[0-9A-Z]{4,5}");
	await page
		.getByLabel("Own error message (optional)")
		.fill("Enter a registration number, e.g. WX12345");

	await page
		.getByLabel("System field to add")
		.selectOption({ label: "First name (employee.firstName)" });
	await page.getByRole("button", { name: "Add system field" }).click();

	// The system field was added last; it goes first.
	await page.getByRole("button", { name: "Move up", exact: true }).click();

	// Page two: a declaration that has to be ticked.
	await page.getByRole("button", { name: "Add page" }).click();
	await page.getByRole("button", { name: /Page 2/ }).click();
	await page.getByLabel("Page title").fill("Statement");
	await page.getByRole("button", { name: "Add question" }).click();
	await page.getByLabel("Label *").fill("I will use the car for work only");
	await page.getByLabel("Type").selectOption({ label: "Yes / no (tick box)" });
	await page.getByLabel("Must be ticked").check({ force: true });

	await docShot(page, "form-builder");

	await page.getByRole("button", { name: "Save draft" }).click();
	await expect(page.getByText("Unsaved changes")).toBeHidden();

	// The preview holds a page to its rules on "Continue", exactly as filling it in will.
	await page.getByRole("tab", { name: "Preview" }).click();
	await expectStep(page, "Vehicle");
	// The field's own message stands in for every rule it breaks, "required" included.
	await page.getByRole("button", { name: "Continue" }).click();
	await expect(page.getByText("Enter a registration number, e.g. WX12345")).toBeVisible();
	await expectStep(page, "Vehicle");

	await page.getByLabel("Registration number").fill("12");
	await expect(page.getByText("Enter a registration number, e.g. WX12345")).toBeVisible();
	await page.getByLabel("Registration number").fill("WX12345");
	await page.getByLabel("First name").fill("Anna");

	await docShot(page, "form-builder-preview");

	await continueTo(page, "Statement");
	await page.getByLabel("I will use the car for work only").check();
	await continueTo(page, "Review");
	await expect(summaryItem(page, "Registration number")).toContainText("WX12345");

	await page.getByRole("button", { name: "Publish" }).click();
	await expect(page.getByText("v1", { exact: true })).toBeVisible();

	await page.getByRole("tab", { name: /Versions/ }).click();
	await expect(page.getByRole("button", { name: /^v1 · / })).toBeVisible();
});

test("adds a system field to the catalogue", async ({ page }) => {
	await open(page, "/app/forms/system-fields");
	await expect(page.getByRole("cell", { name: "employee.pesel" })).toBeVisible();

	await page.getByRole("button", { name: "New system field" }).click();

	const drawer = page.getByRole("dialog", { name: "New system field" });
	await drawer.getByLabel("Code (never changes)").fill("employee.shoeSize");
	await drawer.getByLabel("Type").selectOption({ label: "Number" });
	await drawer.getByLabel("Label").fill("Shoe size");
	await drawer.getByLabel("Minimum").fill("30");
	await drawer.getByLabel("Maximum").fill("52");
	await drawer.getByRole("button", { name: "Save changes" }).click();

	await expect(page.getByRole("cell", { name: "employee.shoeSize" })).toBeVisible();

	await docShot(page, "system-fields");
});
