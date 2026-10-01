import { CalendarDate } from "@internationalized/date";
import { pl } from "date-fns/locale";
import { describe, expect, it } from "vitest";
import {
	boundsError,
	clampDate,
	DEFAULT_YEARS_AHEAD,
	DEFAULT_YEARS_BACK,
	dayKey,
	formatCalendarMonth,
	fromCalendarDate,
	isDateSeparator,
	isYearIncomplete,
	monthNames,
	parseTypedDate,
	segmentsFill,
	toCalendarDate,
	yearOptions,
	yearRangeFor,
	yearsBetween,
} from "./datePickerUtils";

const day = (year: number, month: number, date: number) => new Date(year, month - 1, date);

describe("Date <-> CalendarDate", () => {
	it("keeps the local day, whatever the time of day", () => {
		const value = toCalendarDate(new Date(1959, 2, 7, 23, 59));

		expect(value?.toString()).toBe("1959-03-07");
	});

	it("comes back as local midnight of the same day", () => {
		const date = fromCalendarDate(new CalendarDate(2026, 10, 1));

		expect(date).toEqual(new Date(2026, 9, 1));
		expect(date?.getHours()).toBe(0);
	});

	it("round-trips", () => {
		const date = day(2031, 6, 30);

		expect(fromCalendarDate(toCalendarDate(date))).toEqual(date);
	});

	it("passes a year below 100 through instead of reading it as 19xx", () => {
		expect(fromCalendarDate(new CalendarDate(19, 3, 7))?.getFullYear()).toBe(19);
	});

	it("maps nothing to nothing", () => {
		expect(toCalendarDate(null)).toBeNull();
		expect(toCalendarDate(undefined)).toBeNull();
		expect(toCalendarDate(new Date(Number.NaN))).toBeNull();
		expect(fromCalendarDate(null)).toBeNull();
	});

	it("tells a half-typed year from a whole one", () => {
		expect(isYearIncomplete(new CalendarDate(195, 3, 7))).toBe(true);
		expect(isYearIncomplete(new CalendarDate(1959, 3, 7))).toBe(false);
		expect(isYearIncomplete(null)).toBe(false);
	});

	it("compares days, not timestamps", () => {
		expect(dayKey(new Date(2026, 9, 1, 12))).toBe(dayKey(new Date(2026, 9, 1)));
		expect(dayKey(null)).toBe("");
	});
});

describe("parseTypedDate", () => {
	it.each([
		["07.03.1959", day(1959, 3, 7)],
		["7.3.1959", day(1959, 3, 7)],
		["07031959", day(1959, 3, 7)],
		["07-03-1959", day(1959, 3, 7)],
		["07/03/1959", day(1959, 3, 7)],
		["07 03 1959", day(1959, 3, 7)],
		["1959-03-07", day(1959, 3, 7)],
		["  29.02.2024  ", day(2024, 2, 29)],
	])("takes %s", (text, expected) => {
		expect(parseTypedDate(text)).toEqual(expected);
	});

	it.each([
		"31.02.2026",
		"29.02.2025",
		"32.01.2026",
		"01.13.2026",
		"00.01.2026",
		"07.03.59",
		"7031959",
		"tomorrow",
		"07.03.1959x",
	])("refuses %s", (text) => {
		expect(parseTypedDate(text)).toBeUndefined();
	});

	it("reads empty text as no date", () => {
		expect(parseTypedDate("")).toBeNull();
		expect(parseTypedDate("   ")).toBeNull();
	});
});

describe("isDateSeparator", () => {
	it.each([".", "-", "/", ",", " "])("moves on at %j", (key) => {
		expect(isDateSeparator(key)).toBe(true);
	});

	it.each(["1", "a", "Enter", "Tab"])("does not at %j", (key) => {
		expect(isDateSeparator(key)).toBe(false);
	});
});

describe("segmentsFill", () => {
	it("tells an empty, a half and a whole entry apart", () => {
		expect(segmentsFill([false, false, false])).toBe("empty");
		expect(segmentsFill([true, true, false])).toBe("partial");
		expect(segmentsFill([true, true, true])).toBe("complete");
	});
});

describe("boundsError", () => {
	const minDate = day(2026, 10, 1);
	const maxDate = day(2026, 10, 31);

	it("names the lower limit", () => {
		expect(boundsError(day(2026, 9, 30), minDate, maxDate)).toBe(
			"The date cannot be earlier than 01.10.2026.",
		);
	});

	it("names the upper limit", () => {
		expect(boundsError(day(2026, 11, 1), minDate, maxDate)).toBe(
			"The date cannot be later than 31.10.2026.",
		);
	});

	it("takes the limits themselves, whatever their time of day", () => {
		expect(boundsError(day(2026, 10, 1), new Date(2026, 9, 1, 15), maxDate)).toBeNull();
		expect(boundsError(new Date(2026, 9, 31, 23), minDate, maxDate)).toBeNull();
	});

	it("has nothing to say without limits", () => {
		expect(boundsError(day(1959, 3, 7))).toBeNull();
	});
});

describe("clampDate", () => {
	it("moves today inside the limits for the Today button", () => {
		expect(clampDate(day(2026, 9, 15), day(2026, 10, 1))).toEqual(day(2026, 10, 1));
		expect(clampDate(day(2026, 12, 1), undefined, day(2026, 10, 31))).toEqual(day(2026, 10, 31));
		expect(clampDate(new Date(2026, 9, 5, 18), day(2026, 10, 1))).toEqual(day(2026, 10, 5));
	});
});

describe("year range", () => {
	it("counts from the low year up, either way round", () => {
		expect(yearsBetween(2024, 2026)).toEqual([2024, 2025, 2026]);
		expect(yearsBetween(2026, 2024)).toEqual([2024, 2025, 2026]);
	});

	it("defaults to 100 years back and 20 ahead", () => {
		const years = yearRangeFor({ currentYear: 2026 });

		expect(years[0]).toBe(2026 - DEFAULT_YEARS_BACK);
		expect(years.at(-1)).toBe(2026 + DEFAULT_YEARS_AHEAD);
	});

	it("follows the limits where they are given", () => {
		const years = yearRangeFor({ minDate: day(2020, 5, 1), currentYear: 2026 });

		expect(years[0]).toBe(2020);
		expect(years.at(-1)).toBe(2026 + DEFAULT_YEARS_AHEAD);
	});

	it("lets an explicit range win over the limits", () => {
		const years = yearRangeFor({
			yearRange: { from: 1926, to: 2012 },
			minDate: day(2020, 1, 1),
			maxDate: day(2030, 1, 1),
			currentYear: 2026,
		});

		expect(years[0]).toBe(1926);
		expect(years.at(-1)).toBe(2012);
	});

	it("adds the year in view when the range does not hold it", () => {
		expect(yearOptions([2024, 2025], 2026)).toEqual([2024, 2025, 2026]);
		expect(yearOptions([2024, 2025], 1959)).toEqual([1959, 2024, 2025]);
		expect(yearOptions([2024, 2025], 2025)).toEqual([2024, 2025]);
	});
});

describe("labels", () => {
	it("names the months in the calendar's locale, capitalised", () => {
		const months = monthNames(pl);

		expect(months).toHaveLength(12);
		expect(months[0]).toBe("Styczeń");
		expect(months[9]).toBe("Październik");
	});

	it("titles the month without a year select", () => {
		expect(formatCalendarMonth(day(2026, 9, 1), pl)).toBe("Wrzesień 2026");
	});
});
