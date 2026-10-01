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

type OptionPick = string | { label: string } | { index: number };

const escapeRegExp = (text: string) => text.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");

/**
 * The trigger of a HeroUI `Select` named by its label. React Aria names it "<value> <label>", so
 * the label is matched at the end of the name.
 */
function selectTrigger(scope: Page | Locator, label: string) {
	return scope
		.locator('[data-slot="select-trigger"]')
		.and(scope.getByRole("button", { name: new RegExp(`(^|\\s)${escapeRegExp(label)}$`) }));
}

/**
 * The `<select>` that holds a field's value, whatever draws the field: the panel's own native
 * select, or the hidden one a HeroUI `Select` keeps next to its trigger for forms. Its value is
 * the option's key either way, so `toHaveValue` reads the same for both.
 */
export function selectValue(scope: Page | Locator, label: string) {
	return selectTrigger(scope, label).locator("xpath=..").locator('select[tabindex="-1"]');
}

/**
 * Picks an option in a labelled select. Native selects (countries, languages, lists built by hand)
 * take `selectOption`; a HeroUI `Select` is opened and its option clicked - by key when a string is
 * given, as `selectOption` would match a value. An index counts real options from 1, as in a
 * native select whose option 0 is the placeholder.
 */
export async function chooseOption(scope: Page | Locator, label: string, pick: OptionPick) {
	const page = "page" in scope && typeof scope.page === "function" ? scope.page() : (scope as Page);
	const native = scope
		.getByLabel(label, { exact: true })
		.and(scope.locator('select:not([tabindex="-1"])'));
	const trigger = selectTrigger(scope, label);

	await expect(native.or(trigger).first()).toBeVisible();

	if ((await native.count()) > 0) {
		await native.selectOption(pick);
		return;
	}

	await trigger.click();
	const listbox = page.getByRole("listbox").last();
	const option =
		typeof pick === "string"
			? listbox.locator(`[role="option"][data-key="${pick}"]`)
			: "label" in pick
				? listbox.getByRole("option", { name: pick.label, exact: true })
				: listbox.getByRole("option").nth(pick.index - 1);
	await option.click();
	await expect(listbox).toBeHidden();
}
