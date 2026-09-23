import { describe, expect, it } from "vitest";
import { codePrefix, type Layout, layoutReducer, newFormField, suggestCode } from "./layout";

const layout = (): Layout => {
	const first = newFormField("Text", "Office", "tax.office");
	const second = newFormField("Country", "Residence", "tax.residence");

	return [
		{ pageId: "a", title: "Tax", description: null, fields: [first, second] },
		{ pageId: "b", title: "Consents", description: null, fields: [] },
	];
};

describe("layout reducer", () => {
	it("moves a field one step and stops at the edge", () => {
		const start = layout();
		const [first, second] = start[0].fields;

		const moved = layoutReducer(start, { type: "moveField", fieldId: first.fieldId, by: 1 });
		expect(moved[0].fields.map((f) => f.fieldId)).toEqual([second.fieldId, first.fieldId]);

		const stuck = layoutReducer(moved, { type: "moveField", fieldId: first.fieldId, by: 1 });
		expect(stuck[0].fields.map((f) => f.fieldId)).toEqual([second.fieldId, first.fieldId]);
	});

	it("moves a field to another page, at its end", () => {
		const start = layout();
		const field = start[0].fields[0];

		const moved = layoutReducer(start, {
			type: "moveFieldToPage",
			fieldId: field.fieldId,
			pageId: "b",
		});

		expect(moved[0].fields).toHaveLength(1);
		expect(moved[1].fields.map((f) => f.fieldId)).toEqual([field.fieldId]);
	});

	it("reorders pages", () => {
		const moved = layoutReducer(layout(), { type: "movePage", pageId: "b", by: -1 });

		expect(moved.map((page) => page.pageId)).toEqual(["b", "a"]);
	});

	it("removes a page with its fields", () => {
		expect(
			layoutReducer(layout(), { type: "removePage", pageId: "a" }).map((p) => p.pageId),
		).toEqual(["b"]);
	});
});

describe("codes", () => {
	it("are suggested from the label in camelCase, without Polish letters", () => {
		expect(suggestCode("tax", "Urząd skarbowy")).toBe("tax.urzadSkarbowy");
		expect(suggestCode("gdprConsent", "Zgoda na przetwarzanie")).toBe(
			"gdprConsent.zgodaNaPrzetwarzanie",
		);
		expect(suggestCode("survey", "  ")).toBe("survey.field");
	});

	it("take the form's code as the prefix", () => {
		expect(codePrefix("gdpr-consent")).toBe("gdprConsent");
	});
});
