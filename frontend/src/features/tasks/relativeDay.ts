import { differenceInCalendarDays, format } from "date-fns";

/**
 * "Today · 10:30", "Tomorrow · 09:00", "Yesterday · 16:30", otherwise "Mon 28.09 · 12:00" -
 * what a to-do list reads at a glance. Counted in the browser's own time zone, the same one the
 * API was asked to cut the list in.
 */
export function relativeDayLabel(value: string | Date, now: Date = new Date()): string {
	const date = typeof value === "string" ? new Date(value) : value;
	const time = format(date, "HH:mm");

	switch (differenceInCalendarDays(date, now)) {
		case 0:
			return `Today · ${time}`;
		case 1:
			return `Tomorrow · ${time}`;
		case -1:
			return `Yesterday · ${time}`;
		default:
			return `${format(date, "EEE dd.MM")} · ${time}`;
	}
}

/** The zone the browser runs in, which is what "today" means to the person looking. */
export function browserTimeZone(): string {
	try {
		return Intl.DateTimeFormat().resolvedOptions().timeZone || "Europe/Warsaw";
	} catch {
		return "Europe/Warsaw";
	}
}
