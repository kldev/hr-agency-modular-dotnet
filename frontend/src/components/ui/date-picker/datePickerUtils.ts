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
