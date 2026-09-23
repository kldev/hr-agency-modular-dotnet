import { describe, expect, it } from "vitest";
import type { FormField, FormPage } from "#/api/models";
import { defaultValues, fromControl, parseNumber, toAnswers, toControl } from "./answerValues";
import { buildAnswerSchema, pageKeys } from "./buildAnswerSchema";
import { keyOf } from "./fieldKeys";

const field = (code: string, type: FormField["type"], required = false): FormField => ({
	fieldId: crypto.randomUUID(),
	source: "Form",
	systemFieldId: null,
	code,
	type,
	label: code,
	labelOverride: null,
	description: null,
	placeholder: null,
	rules: {
		required,
		minLength: null,
		maxLength: null,
		pattern: null,
		min: null,
		max: null,
		decimals: null,
		minDate: null,
		maxDate: null,
		minSelected: null,
		maxSelected: null,
		message: null,
	},
	options: [],
	defaultValue: null,
	visibleWhen: null,
});

describe("field keys", () => {
	it("never contain a dot, whatever the code", () => {
		const first = field("employee.firstName", "Text");
		const last = field("employee.lastName", "Text");

		expect(keyOf(first)).not.toContain(".");
		expect(keyOf(first)).not.toBe(keyOf(last));
	});
});

describe("control values", () => {
	it("read a comma as a decimal point", () => {
		expect(parseNumber("1,5")).toBe(1.5);
		expect(parseNumber("12 000")).toBe(12000);
		expect(parseNumber("abc")).toBeNull();
	});

	it("keep an unreadable number as text, so it is refused as the wrong kind", () => {
		expect(fromControl({ type: "Number" }, "abc")).toMatchObject({ text: "abc", number: null });
	});

	it("turn an unticked box into no answer", () => {
		expect(fromControl({ type: "Boolean" }, false)).toBeNull();
	});

	it("carry a date through the picker's noon time and back", () => {
		const control = toControl(
			{ type: "Date" },
			{
				text: null,
				number: null,
				date: "2026-09-23",
				boolean: null,
				values: null,
			},
		);

		expect(control).toBe("2026-09-23T12:00");
		expect(fromControl({ type: "Date" }, control)?.date).toBe("2026-09-23");
	});
});

describe("a whole form", () => {
	const consent = field("gdpr.consent", "Boolean", true);
	const office = field("tax.office", "Text", true);
	const pages: FormPage[] = [
		{ pageId: "p1", title: "Tax", description: null, fields: [office] },
		{ pageId: "p2", title: "Consents", description: null, fields: [consent] },
	];

	it("starts from the answers given so far", () => {
		const values = defaultValues(pages, [
			{
				fieldCode: "tax.office",
				value: { text: "US Mokotów", number: null, date: null, boolean: null, values: null },
			},
		]);

		expect(values[keyOf(office)]).toBe("US Mokotów");
		expect(values[keyOf(consent)]).toBe(false);
	});

	it("sends only what is filled in, named by code", () => {
		const answers = toAnswers(pages, { [keyOf(office)]: "US Mokotów", [keyOf(consent)]: false });

		expect(answers).toEqual([
			{ fieldCode: "tax.office", value: expect.objectContaining({ text: "US Mokotów" }) },
		]);
	});

	it("lets a draft through but not a submission", () => {
		const values = defaultValues(pages);

		expect(buildAnswerSchema(pages, "draft").safeParse(values).success).toBe(true);

		const submitted = buildAnswerSchema(pages, "submit").safeParse(values);

		expect(submitted.success).toBe(false);
		expect(submitted.error?.issues.map((issue) => issue.path[0])).toEqual([
			keyOf(office),
			keyOf(consent),
		]);
	});

	it("names the keys of one page", () => {
		expect(pageKeys(pages[1])).toEqual([keyOf(consent)]);
	});
});
