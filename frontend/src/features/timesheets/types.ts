import type { OrganizationRole, TimeSheetStatus } from "@/api/models";

export const timeSheetStatuses: Record<TimeSheetStatus, string> = {
	Draft: "Draft",
	Submitted: "Submitted",
	Approved: "Approved",
	Correction: "For correction",
	Settled: "Settled",
};

export const timeSheetStatusClass: Record<TimeSheetStatus, string> = {
	Draft: "badge-draft",
	Submitted: "badge-viewed",
	Approved: "badge-won",
	Correction: "badge-suspended",
	Settled: "badge-completed",
};

/**
 * Mirrors `Agency/Domain/TimeSheets/TimeSheetStatusChangePolicy.cs`. There is no "rejected": hours
 * are either corrected or agreed, so a month handed back goes to `Correction` and comes round again.
 */
export const allowedTimeSheetTransitions: Record<TimeSheetStatus, TimeSheetStatus[]> = {
	Draft: ["Submitted"],
	Submitted: ["Approved", "Correction"],
	Approved: ["Settled", "Correction"],
	Correction: ["Submitted"],
	Settled: [],
};

/** Mirrors `TimeSheetStatusChangePolicy.IsEditable`. A month nobody has opened yet is editable too. */
export function isTimeSheetEditable(status: TimeSheetStatus | null | undefined): boolean {
	return status === undefined || status === null || status === "Draft" || status === "Correction";
}

/**
 * Mirrors `Api/Auth/PayrollPolicy.cs`, and it is the only role this front end asks about. Approving
 * a month is deliberately *not* here: that follows from the chart, and a role that could approve
 * anybody's hours would make the structure decorative. The backend remains the authority - hiding
 * the tab only saves somebody a click into a screen that would answer 403.
 */
export function isPayroll(role: OrganizationRole | undefined | null): boolean {
	return role === "HumanResources" || role === "Admin";
}

/* Mirrors `Domain/TimeSheets/WorkDuration.cs`: minutes come in fives, and an hour is 60 of them. */
export const minuteStep = 5;

export const hourOptions = Array.from({ length: 25 }, (_, hour) => hour);

export const minuteOptions = Array.from(
	{ length: 60 / minuteStep },
	(_, index) => index * minuteStep,
);

/** "8h 30m", and an em dash where there is nothing - a zero would read as a recorded empty day. */
export function formatMinutes(minutes: number): string {
	if (!minutes) return "—";

	const hours = Math.floor(minutes / 60);
	const rest = minutes % 60;

	if (!rest) return `${hours}h`;
	if (!hours) return `${rest}m`;

	return `${hours}h ${rest}m`;
}

export function formatHoursOnly(minutes: number): string {
	return (minutes / 60).toFixed(minutes % 60 === 0 ? 0 : 1);
}

/*
 * Mirrors `TimeSheetRules.NotCoveredMessage` and `TimeSheetRules.NoEmploymentMessage`. Worth saying
 * before the backend gets a chance to: both are preconditions shown informatively, and an empty
 * calendar that refuses every click explains nothing.
 */
export const notCoveredMessage =
	"This contract carries no duty to record hours, so there is nothing to fill in here.";

export const noEmploymentMessage =
	"There is no employment record for you yet, so there is no month to record hours against.";

export const submitWarning =
	"Once it is sent, the month is locked until somebody sends it back for correction.";

/* ---- the month the page is looking at ------------------------------------------------------ */

export type MonthInView = { year: number; month: number };

export function currentMonth(): MonthInView {
	const today = new Date();

	return { year: today.getFullYear(), month: today.getMonth() + 1 };
}

export function shiftMonth({ year, month }: MonthInView, by: number): MonthInView {
	const shifted = new Date(year, month - 1 + by, 1);

	return { year: shifted.getFullYear(), month: shifted.getMonth() + 1 };
}

export function monthLabel({ year, month }: MonthInView): string {
	return new Date(year, month - 1, 1).toLocaleDateString(undefined, {
		month: "long",
		year: "numeric",
	});
}

/** Mirrors `TimeSheetPeriod.HasStartedBy`: the current month counts as started, the next does not. */
export function isFutureMonth({ year, month }: MonthInView): boolean {
	const now = currentMonth();

	return year > now.year || (year === now.year && month > now.month);
}

/**
 * How many days somebody could have filled in. Weekends only - we hold no holiday calendar, and
 * inventing one here would make the denominator disagree with everybody's actual obligation.
 */
export function workingDaysInMonth({ year, month }: MonthInView): number {
	const days = new Date(year, month, 0).getDate();

	let count = 0;

	for (let day = 1; day <= days; day++) {
		const weekday = new Date(year, month - 1, day).getDay();

		if (weekday !== 0 && weekday !== 6) count++;
	}

	return count;
}

/** The API takes and returns plain `yyyy-MM-dd`; `new Date(...)` on that string is UTC-shifted. */
export function toDateKey(date: Date): string {
	const month = String(date.getMonth() + 1).padStart(2, "0");
	const day = String(date.getDate()).padStart(2, "0");

	return `${date.getFullYear()}-${month}-${day}`;
}

export function fromDateKey(value: string): Date {
	const [year, month, day] = value.split("-").map(Number);

	return new Date(year, month - 1, day);
}

/** `TimeOnly` arrives as `HH:mm:ss`; the picker and the cells both want `HH:mm`. */
export function toTimeOfDay(value: string): string {
	return value.slice(0, 5);
}
