import type { Locale } from "date-fns";
import {
	addDays,
	endOfMonth,
	endOfWeek,
	format,
	isAfter,
	isBefore,
	isSameDay,
	isSameMonth,
	startOfMonth,
	startOfWeek,
} from "date-fns";

export interface CalendarDay {
	date: Date;
	currentMonth: boolean;
}

export function isDateDisabled(date: Date, minDate?: Date, maxDate?: Date): boolean {
	if (minDate && isBefore(date, minDate)) {
		return true;
	}

	if (maxDate && isAfter(date, maxDate)) {
		return true;
	}

	return false;
}

export function isSelectedDate(date: Date, selectedDate?: Date | null): boolean {
	return Boolean(selectedDate && isSameDay(date, selectedDate));
}

export function isToday(date: Date, today: Date): boolean {
	return isSameDay(date, today);
}

export function createCalendarDays(month: Date, locale: Locale): CalendarDay[] {
	const monthStart = startOfMonth(month);
	const monthEnd = endOfMonth(month);

	const calendarStart = startOfWeek(monthStart, {
		locale,
		weekStartsOn: 1,
	});

	const calendarEnd = endOfWeek(monthEnd, {
		locale,
		weekStartsOn: 1,
	});

	const days: CalendarDay[] = [];

	let current = calendarStart;

	while (current <= calendarEnd) {
		days.push({
			date: current,
			currentMonth: isSameMonth(current, month),
		});

		current = addDays(current, 1);
	}

	return days;
}

export function createWeekDays(referenceDate: Date, locale: Locale): Date[] {
	const start = startOfWeek(referenceDate, {
		locale,
		weekStartsOn: 1,
	});

	return Array.from({ length: 7 }, (_, index) => addDays(start, index));
}

export function formatCalendarMonth(date: Date, locale: Locale): string {
	const value = format(date, "LLLL yyyy", {
		locale,
	});

	return value.charAt(0).toUpperCase() + value.slice(1);
}

export function formatCalendarWeekday(date: Date, locale: Locale): string {
	return format(date, "EEEEE", {
		locale,
	});
}

export function toIsoDate(date: Date | null | undefined): string | null {
	if (!date) {
		return null;
	}

	return format(date, "yyyy-MM-dd");
}

export function fromIsoDate(value?: string | null): Date | null {
	if (!value) {
		return null;
	}

	const [year, month, day] = value.split("-").map(Number);

	if (!year || !month || !day || month < 1 || month > 12 || day < 1 || day > 31) {
		return null;
	}

	const date = new Date(year, month - 1, day);

	if (date.getFullYear() !== year || date.getMonth() !== month - 1 || date.getDate() !== day) {
		return null;
	}

	return date;
}

export function normalizeDate(date: Date): Date {
	return new Date(date.getFullYear(), date.getMonth(), date.getDate());
}

export function clampDate(date: Date, minDate?: Date, maxDate?: Date): Date {
	let result = normalizeDate(date);

	if (minDate && isBefore(result, minDate)) {
		result = normalizeDate(minDate);
	}

	if (maxDate && isAfter(result, maxDate)) {
		result = normalizeDate(maxDate);
	}

	return result;
}

/**
 * A date as somebody types it: "22.09.2026", with a dot, a slash, a dash or a space between the
 * parts, eight bare digits ("22092026"), or ISO ("2026-09-22") for whoever pastes one. A two digit
 * year is refused rather than guessed - "26" is 1926 on a birth date and 2026 on a contract.
 *
 * `null` for an empty field, `undefined` for text that is not a real day (31.02 included).
 */
export function parseTypedDate(text: string): Date | null | undefined {
	const value = text.trim();

	if (!value) {
		return null;
	}

	const dayFirst = /^(\d{1,2})[.\-/ ](\d{1,2})[.\-/ ](\d{4})$/.exec(value);
	const bare = /^(\d{2})(\d{2})(\d{4})$/.exec(value);
	const iso = /^(\d{4})-(\d{1,2})-(\d{1,2})$/.exec(value);

	const parts = dayFirst ?? bare;

	const [day, month, year] = parts
		? [Number(parts[1]), Number(parts[2]), Number(parts[3])]
		: iso
			? [Number(iso[3]), Number(iso[2]), Number(iso[1])]
			: [0, 0, 0];

	if (!year || month < 1 || month > 12 || day < 1) {
		return undefined;
	}

	const date = new Date(year, month - 1, day);

	// `new Date` rolls 31.02 over into March; a date that moved was not a date.
	return date.getFullYear() === year && date.getMonth() === month - 1 && date.getDate() === day
		? date
		: undefined;
}

/**
 * Puts the dots in while somebody types digits, so "22092026" reads as "22.09.2026" on the way.
 * Only when the text grew - adding a dot on a backspace would make the dot impossible to delete.
 */
export function withDateSeparators(next: string, previous: string): string {
	if (next.length <= previous.length) {
		return next;
	}

	return /^\d{2}$/.test(next) || /^\d{1,2}\.\d{2}$/.test(next) ? `${next}.` : next;
}

/** The years a year select offers, newest last. */
export function yearsBetween(from: number, to: number): number[] {
	const [low, high] = from <= to ? [from, to] : [to, from];

	return Array.from({ length: high - low + 1 }, (_, index) => low + index);
}
