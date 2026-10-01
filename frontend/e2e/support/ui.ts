import { expect, type Locator, type Page } from "@playwright/test";

/**
 * Chooses an item in a `SuggestionPicker` (company, user, team...). The input is a combobox and
 * the options live in a React Aria popover portaled to `<body>`, so they are looked up on the
 * page, not inside the form.
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

/**
 * Types a day into a `DatePicker` the way a user would, as dd.mm.yyyy. The field is HeroUI's
 * segmented `DateField`: a group named by its label, one spinbutton per part, so the day segment
 * is focused and the keys go to the page - each segment moves on by itself, and a dot moves on
 * too. A whole day is taken as soon as the year is typed; there is nothing to commit.
 */
export async function fillDate(scope: Page | Locator, label: string, day: string) {
	const page = "page" in scope && typeof scope.page === "function" ? scope.page() : (scope as Page);
	const field = scope.getByRole("group", { name: label, exact: true });

	await field.locator('[data-type="day"]').click();
	await page.keyboard.type(day);
	await expect(field.locator('[data-slot="date-input-group-input"]')).toHaveText(day);
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
 * The `<select>` that holds a HeroUI `Select`'s value: the hidden one it keeps next to its trigger
 * for forms. Its value is the option's key, so `toHaveValue` reads like a native select's.
 */
export function selectValue(scope: Page | Locator, label: string) {
	return selectTrigger(scope, label).locator("xpath=..").locator('select[tabindex="-1"]');
}

/**
 * Picks an option in a labelled select: a HeroUI `Select` (enums, lists built by hand, languages)
 * or a combo box over a static list (countries), opened from its chevron so the whole list shows.
 * A string picks by key, as `selectOption` matched a value; a label by the option's name; an index
 * counts the options from 1 as they are listed - a placeholder is not an option.
 */
export async function chooseOption(scope: Page | Locator, label: string, pick: OptionPick) {
	const page = "page" in scope && typeof scope.page === "function" ? scope.page() : (scope as Page);
	const trigger = selectTrigger(scope, label);
	const combobox = scope.getByRole("combobox", { name: label, exact: true });

	await expect(trigger.or(combobox).first()).toBeVisible();

	if ((await combobox.count()) > 0) {
		await combobox.locator("xpath=..").locator('[data-slot="combo-box-trigger"]').click();
	} else {
		await trigger.click();
	}

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
