import { currentMonth } from "./types";

export const timeSheetTabs = ["mine", "team", "approvals", "settlement"] as const;

export type TimeSheetTab = (typeof timeSheetTabs)[number];

export type TimeSheetsSearch = {
	year?: number;
	month?: number;
	tab?: TimeSheetTab;
	/** The sheet open in the panel next to the approval queue. */
	person?: string;
};

/**
 * The month is the context of the whole page, so it belongs in the address rather than in state:
 * switching from "my hours" to "the team" must not quietly move everybody back to today, and a
 * link to somebody's August has to be a link.
 */
export function validateTimeSheetsSearch(search: Record<string, unknown>): TimeSheetsSearch {
	const year = Number(search.year);
	const month = Number(search.month);

	const valid = Number.isInteger(year) && Number.isInteger(month) && month >= 1 && month <= 12;

	return {
		year: valid ? year : undefined,
		month: valid ? month : undefined,
		tab: timeSheetTabs.includes(search.tab as TimeSheetTab)
			? (search.tab as TimeSheetTab)
			: undefined,
		person: typeof search.person === "string" && search.person ? search.person : undefined,
	};
}

/** What the page looks at when the address says nothing: this month. */
export function monthFromSearch(search: TimeSheetsSearch) {
	return search.year && search.month ? { year: search.year, month: search.month } : currentMonth();
}
