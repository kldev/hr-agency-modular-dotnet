import { CalendarDate, type DateValue } from "@internationalized/date";
import type { Locale } from "date-fns";
import { format, isAfter, isBefore } from "date-fns";

/** How far back and ahead a year select reaches when neither a range nor a limit says otherwise. */
export const DEFAULT_YEARS_BACK = 100;
export const DEFAULT_YEARS_AHEAD = 20;

export const NOT_A_DATE_MESSAGE = "Not a date - type it as dd.mm.yyyy.";

/*
 * The form speaks `Date` (local midnight), HeroUI speaks `CalendarDate`. A `CalendarDate` has no
 * time and no zone, so the conversion goes through the local year, month and day and never
 * through a timestamp - `toDate("UTC")` would move a Polish midnight to the day before.
 */

export function toCalendarDate(date: Date | null | undefined): CalendarDate | null {
	if (!date || Number.isNaN(date.getTime())) {
		return null;
	}

	return new CalendarDate(date.getFullYear(), date.getMonth() + 1, date.getDate());
}

export function fromCalendarDate(value: DateValue | null | undefined): Date | null {
	if (!value) {
		return null;
	}

	const date = new Date(2000, value.month - 1, value.day);

	// Not the constructor: `new Date(19, ...)` is 1919, and a year is passed through as it is.
	date.setFullYear(value.year);

	return date;
}

/**
 * Whether the year is still being typed. A segment takes a year digit by digit and reports the
 * date after each one - 0001, 0019, 0195, 1959 - so a year below 1000 is a half-typed one: it is
 * neither handed to the form nor checked against the limits, and leaving the field with it is
 * an error. Nobody here is born, hired or posted before the year 1000.
 */
export function isYearIncomplete(value: DateValue | null | undefined): boolean {
	return Boolean(value && value.year < 1000);
}

/** A key that says whether two dates are the same day, whatever their time of day. */
export function dayKey(date: Date | null | undefined): string {
	return date ? `${date.getFullYear()}-${date.getMonth() + 1}-${date.getDate()}` : "";
}

export function normalizeDate(date: Date): Date {
	return new Date(date.getFullYear(), date.getMonth(), date.getDate());
}

export function isDateDisabled(date: Date, minDate?: Date, maxDate?: Date): boolean {
	return Boolean(
		(minDate && isBefore(normalizeDate(date), normalizeDate(minDate))) ||
			(maxDate && isAfter(normalizeDate(date), normalizeDate(maxDate))),
	);
}

/** Today, or the nearest limit when today is outside them - what the "Today" button picks. */
export function clampDate(date: Date, minDate?: Date, maxDate?: Date): Date {
	let result = normalizeDate(date);

	if (minDate && isBefore(result, normalizeDate(minDate))) {
		result = normalizeDate(minDate);
	}

	if (maxDate && isAfter(result, normalizeDate(maxDate))) {
		result = normalizeDate(maxDate);
	}

	return result;
}

/** The panel's way of writing a day, whatever locale the calendar speaks. */
export function formatDay(date: Date): string {
	return format(date, "dd.MM.yyyy");
}

/**
 * Why a typed day is refused, naming the limit it crosses. A typed date is never quietly moved
 * inside the limits: somebody who typed 1959 meant 1959, and a field that changes it to the
 * earliest allowed day hides the mistake instead of pointing at it.
 */
export function boundsError(date: Date, minDate?: Date, maxDate?: Date): string | null {
	if (minDate && isBefore(normalizeDate(date), normalizeDate(minDate))) {
		return `The date cannot be earlier than ${formatDay(minDate)}.`;
	}

	if (maxDate && isAfter(normalizeDate(date), normalizeDate(maxDate))) {
		return `The date cannot be later than ${formatDay(maxDate)}.`;
	}

	return null;
}

/**
 * A date as somebody types or pastes it: "22.09.2026", with a dot, a slash, a dash or a space
 * between the parts, eight bare digits ("22092026"), or ISO ("2026-09-22") for whoever pastes one.
 * A two digit year is refused rather than guessed - "26" is 1926 on a birth date and 2026 on a
 * contract.
 *
 * `null` for an empty text, `undefined` for text that is not a real day (31.02 included).
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
 * The keys that end a part of the date while it is typed. A segmented field moves on by itself
 * after "07" (and after "4", which no second digit can follow), but after "1" it waits for one
 * more - so "1.03.1959" has to move on at the dot, or the "0" lands in the day and makes it 10.
 */
export function isDateSeparator(key: string): boolean {
	return key === "." || key === "-" || key === "/" || key === "," || key === " ";
}

/** What the segments of a date field hold, read from their `data-placeholder` marks. */
export type SegmentsFill = "empty" | "partial" | "complete";

export function segmentsFill(filled: readonly boolean[]): SegmentsFill {
	const count = filled.filter(Boolean).length;

	return count === 0 ? "empty" : count === filled.length ? "complete" : "partial";
}

/** The years a year select offers, oldest first. */
export function yearsBetween(from: number, to: number): number[] {
	const [low, high] = from <= to ? [from, to] : [to, from];

	return Array.from({ length: high - low + 1 }, (_, index) => low + index);
}

/**
 * The years of the year select: `yearRange` when given, otherwise `minDate`..`maxDate`, and
 * where a limit is missing, 100 years back and 20 ahead of the current year - which covers a
 * date of birth and the validity of a document alike.
 */
export function yearRangeFor(options: {
	yearRange?: { from: number; to: number };
	minDate?: Date;
	maxDate?: Date;
	currentYear: number;
}): number[] {
	const { yearRange, minDate, maxDate, currentYear } = options;

	return yearsBetween(
		yearRange?.from ?? minDate?.getFullYear() ?? currentYear - DEFAULT_YEARS_BACK,
		yearRange?.to ?? maxDate?.getFullYear() ?? currentYear + DEFAULT_YEARS_AHEAD,
	);
}

/** The years in the select, with the one in view added when the range does not hold it. */
export function yearOptions(years: readonly number[], visibleYear: number): number[] {
	return (years.includes(visibleYear) ? [...years] : [...years, visibleYear]).sort((a, b) => a - b);
}

function capitalise(value: string): string {
	return value.charAt(0).toUpperCase() + value.slice(1);
}

/** The twelve month names for the month select, in the calendar's locale, January first. */
export function monthNames(locale: Locale): string[] {
	return Array.from({ length: 12 }, (_, month) =>
		capitalise(format(new Date(2000, month, 1), "LLLL", { locale })),
	);
}

/** "Wrzesień 2026" - the calendar heading when there is no year select. */
export function formatCalendarMonth(date: Date, locale: Locale): string {
	return capitalise(format(date, "LLLL yyyy", { locale }));
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
