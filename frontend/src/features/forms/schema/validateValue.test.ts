import { describe, expect, it } from "vitest";
import type { FieldValue, FormField } from "#/api/models";
import file from "../../../../../tests/fixtures/forms-validation-cases.json";
import { normalizeValue, type ValidationMode, validateValue } from "./validateValue";

type Case = {
	name: string;
	field: {
		type: FormField["type"];
		rules: Partial<FormField["rules"]>;
		options: FormField["options"];
	};
	value: Partial<FieldValue> | null;
	mode: ValidationMode;
	expected: string | null;
};

const withSlots = (value: Partial<FieldValue> | null): FieldValue | null =>
	value ? { text: null, number: null, date: null, boolean: null, values: null, ...value } : null;

/*
 * The same file FormAnswersValidatorCasesTests reads in xUnit. A case passing there and failing here
 * means the two implementations no longer agree.
 */
describe("validateValue against the shared cases", () => {
	it.each((file.cases as Case[]).map((c) => [c.name, c] as const))("%s", (_, c) => {
		const field = {
			type: c.field.type,
			rules: { required: false, ...c.field.rules } as FormField["rules"],
			options: c.field.options,
		};

		const error = validateValue(field, normalizeValue(field.type, withSlots(c.value)), c.mode);

		expect(error?.code ?? null).toBe(c.expected);

		if (c.expected && c.field.rules.message) {
			expect(error?.message).toBe(c.field.rules.message);
		}
	});
});
