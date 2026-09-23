import { expect, type Locator, type Page } from "@playwright/test";

/**
 * Chooses an item in a `SuggestionPicker` (company, user, team...). The input is a combobox and
 * the options are portaled to `<body>`, so they are looked up on the page, not inside the form.
 */
export async function chooseSuggestion(
	input: Locator,
	query: string,
	option: string | RegExp = query,
) {
	await input.fill(query);
	await input.page().getByRole("listbox").getByRole("option", { name: option }).first().click();
}

/** `chooseSuggestion` for a picker named by its label. */
export async function pickSuggestion(
	scope: Page | Locator,
	label: string,
	query: string,
	option: string | RegExp = query,
) {
	await chooseSuggestion(scope.getByRole("combobox", { name: label, exact: true }), query, option);
}

/** A `FormWizard` step is on screen once its section heading is. */
export async function expectStep(scope: Page | Locator, title: string) {
	await expect(scope.getByRole("heading", { name: title, level: 2, exact: true })).toBeVisible();
}

/** Clicks "Continue" and waits for the next step to render. */
export async function continueTo(scope: Page | Locator, title: string) {
	await scope.getByRole("button", { name: "Continue" }).click();
	await expectStep(scope, title);
}

/** Types a day into a `DatePicker` the way a user would, as dd.mm.yyyy, and commits it. */
export async function fillDate(scope: Page | Locator, label: string, day: string) {
	const input = scope.getByRole("textbox", { name: label, exact: true });

	await input.fill(day);
	await input.press("Enter");
	await expect(input).toHaveValue(day);
}

/** A `SummaryItem` on a wizard's review step: the label and the value it answers. */
export function summaryItem(scope: Page | Locator, label: string) {
	return scope.getByRole("group", { name: label, exact: true });
}
