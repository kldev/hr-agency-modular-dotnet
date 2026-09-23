/**
 * How far back a report looks. Whole calendar months ending with the current one, the way the
 * reports service counts them; the API takes the first and the last month as yyyy-MM.
 */
export type ReportRange = "3" | "6" | "12";

export const reportRanges: Record<ReportRange, string> = {
	"3": "3 months",
	"6": "6 months",
	"12": "12 months",
};

export const defaultReportRange: ReportRange = "6";

/** The router parses search values as JSON, so `?range=12` arrives as the number 12. */
export function parseReportRange(value: unknown): ReportRange | undefined {
	const text = typeof value === "number" ? String(value) : value;

	return typeof text === "string" && text in reportRanges ? (text as ReportRange) : undefined;
}

export interface ReportPeriod {
	from: string;
	to: string;
}

/** yyyy-MM of a month counted back from `today`; months, not days, so no date library needed. */
function monthOf(today: Date, back: number): string {
	const date = new Date(Date.UTC(today.getUTCFullYear(), today.getUTCMonth() - back, 1));

	return `${date.getUTCFullYear()}-${String(date.getUTCMonth() + 1).padStart(2, "0")}`;
}

export function reportPeriod(range: ReportRange, today = new Date()): ReportPeriod {
	return { from: monthOf(today, Number(range) - 1), to: monthOf(today, 0) };
}

/** "Apr 2026" for an axis or a heading - short enough for twelve ticks. */
export function formatMonth(month: string): string {
	const [year, value] = month.split("-").map(Number);

	return new Date(Date.UTC(year, value - 1, 1)).toLocaleDateString("en-GB", {
		month: "short",
		year: "numeric",
		timeZone: "UTC",
	});
}

/** A download link through the panel's own proxy, which adds the credential from the cookie. */
export function exportHref(path: string, period: ReportPeriod): string {
	return `${path}?${new URLSearchParams({ from: period.from, to: period.to })}`;
}
