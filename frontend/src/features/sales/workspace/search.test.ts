import { describe, expect, it } from "vitest";
import { parseWorkspaceSearch, toApiRange } from "./search";

describe("parseWorkspaceSearch", () => {
	it("keeps what it knows", () => {
		expect(parseWorkspaceSearch({ companyId: "c1", tab: "projects", range: "month" })).toEqual({
			companyId: "c1",
			tab: "projects",
			range: "month",
		});
	});

	it("drops what it does not, so the defaults apply", () => {
		expect(parseWorkspaceSearch({ companyId: "", tab: "tasks", range: 7 })).toEqual({
			companyId: undefined,
			tab: undefined,
			range: undefined,
		});
	});
});

describe("toApiRange", () => {
	it("names the range the way the API enum does", () => {
		expect(toApiRange("day")).toBe("Day");
		expect(toApiRange("week")).toBe("Week");
		expect(toApiRange("month")).toBe("Month");
	});
});
