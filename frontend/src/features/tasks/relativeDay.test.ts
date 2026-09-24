import { describe, expect, it } from "vitest";
import { relativeDayLabel } from "./relativeDay";

// Local-time constructors, so the test means the same in any time zone it runs in.
const now = new Date(2026, 8, 24, 10, 0);

describe("relativeDayLabel", () => {
	it("names today, tomorrow and yesterday", () => {
		expect(relativeDayLabel(new Date(2026, 8, 24, 13, 0), now)).toBe("Today · 13:00");
		expect(relativeDayLabel(new Date(2026, 8, 25, 9, 0), now)).toBe("Tomorrow · 09:00");
		expect(relativeDayLabel(new Date(2026, 8, 23, 16, 30), now)).toBe("Yesterday · 16:30");
	});

	it("counts calendar days, not 24 hour spans", () => {
		expect(relativeDayLabel(new Date(2026, 8, 25, 0, 5), new Date(2026, 8, 24, 23, 55))).toBe(
			"Tomorrow · 00:05",
		);
	});

	it("dates anything further away", () => {
		expect(relativeDayLabel(new Date(2026, 8, 28, 12, 0), now)).toBe("Mon 28.09 · 12:00");
		expect(relativeDayLabel(new Date(2026, 8, 21, 9, 0), now)).toBe("Mon 21.09 · 09:00");
	});

	it("reads an ISO string from the API", () => {
		const due = new Date(2026, 8, 24, 13, 0);

		expect(relativeDayLabel(due.toISOString(), now)).toBe("Today · 13:00");
	});
});
