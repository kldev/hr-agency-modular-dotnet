import { Time } from "@internationalized/date";
import { describe, expect, it } from "vitest";
import { formatTimeOfDay, minutesToTime, parseTimeOfDay, timeError } from "./timeInputUtils";

describe("time of day", () => {
	it.each([
		["08:30", 8, 30],
		["8:05", 8, 5],
		["23:55", 23, 55],
		["00:00", 0, 0],
		["08:30:00", 8, 30],
	])("reads %s", (text, hour, minute) => {
		const time = parseTimeOfDay(text);

		expect(time?.hour).toBe(hour);
		expect(time?.minute).toBe(minute);
	});

	it.each(["", "24:00", "08:60", "0830", "eight"])("refuses %j", (text) => {
		expect(parseTimeOfDay(text)).toBeNull();
	});

	it("writes HH:mm", () => {
		expect(formatTimeOfDay(new Time(8, 5))).toBe("08:05");
		expect(formatTimeOfDay(minutesToTime(23 * 60 + 55))).toBe("23:55");
	});
});

describe("timeError", () => {
	it("takes a time on the step inside the office day", () => {
		expect(timeError(new Time(8, 0))).toBeNull();
		expect(timeError(new Time(22, 0))).toBeNull();
		expect(timeError(new Time(13, 45))).toBeNull();
	});

	it("names the window it falls outside", () => {
		expect(timeError(new Time(7, 55))).toBe("The time must be between 08:00 and 22:00.");
		expect(timeError(new Time(22, 5))).toBe("The time must be between 08:00 and 22:00.");
	});

	it("follows a widened window", () => {
		expect(timeError(new Time(23, 0), 0, 23 * 60 + 55)).toBeNull();
		expect(timeError(new Time(0, 0), 0, 23 * 60 + 55)).toBeNull();
	});

	it("refuses minutes between two steps", () => {
		expect(timeError(new Time(8, 33))).toBe("Minutes go in steps of 5.");
	});
});
