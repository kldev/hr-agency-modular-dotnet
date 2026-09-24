import type { FieldAnswer, FieldValue, FormField, FormPage } from "#/api/models";
import { toDateOnly } from "#/utlis/formatRecord";
import { keyOf } from "./fieldKeys";

/**
 * What a control holds, per field type. Strings for everything typed (the form keeps "" rather than
 * null - `.docs/06-front.md`), a boolean for a tick box, a list for a multiple choice. A date is the
 * `YYYY-MM-DDT12:00` string `FormDatePicker` works with; a number is the text somebody typed, so
 * "1,5" survives until it is read.
 */
export type ControlValue = string | boolean | string[];

export type AnswerValues = Record<string, ControlValue>;

export function emptyControl(field: Pick<FormField, "type">): ControlValue {
	switch (field.type) {
		case "Boolean":
			return false;
		case "MultiChoice":
			return [];
		default:
			return "";
	}
}

export function toControl(field: Pick<FormField, "type">, value: FieldValue | null | undefined) {
	if (!value) {
		return emptyControl(field);
	}

	switch (field.type) {
		case "Boolean":
			return value.boolean ?? false;
		case "MultiChoice":
			return [...(value.values ?? [])];
		case "Number":
			return value.number == null ? "" : String(value.number);
		case "Date":
			return value.date ? `${value.date}T12:00` : "";
		default:
			return value.text ?? "";
	}
}

/** Reads a typed number the way people type one here: a comma is a decimal point. */
export function parseNumber(text: string): number | null {
	const normalized = text.trim().replace(/\s/g, "").replace(",", ".");

	if (!/^-?\d+(\.\d+)?$/.test(normalized)) {
		return null;
	}

	return Number(normalized);
}

const emptyValue: FieldValue = {
	text: null,
	number: null,
	date: null,
	boolean: null,
	values: null,
};

/**
 * The answer a control stands for, or null when it is empty. A number that does not parse is kept as
 * text, which the validator then refuses as the wrong kind of answer - exactly what the backend would.
 */
export function fromControl(
	field: Pick<FormField, "type">,
	control: ControlValue | undefined,
): FieldValue | null {
	if (control === undefined) {
		return null;
	}

	switch (field.type) {
		case "Boolean":
			return control === true ? { ...emptyValue, boolean: true } : null;
		case "MultiChoice":
			return Array.isArray(control) && control.length > 0
				? { ...emptyValue, values: control }
				: null;
		case "Number": {
			const text = String(control).trim();

			if (text === "") {
				return null;
			}

			const number = parseNumber(text);

			return number === null ? { ...emptyValue, text } : { ...emptyValue, number };
		}
		case "Date":
			return control ? { ...emptyValue, date: toDateOnly(String(control)) } : null;
		default:
			return String(control).trim() === "" ? null : { ...emptyValue, text: String(control) };
	}
}

export function fieldsOf(pages: readonly FormPage[]): FormField[] {
	return pages.flatMap((page) => page.fields);
}

/** Starting values of a form: every field present, filled from the answers given so far. */
export function defaultValues(
	pages: readonly FormPage[],
	answers: readonly FieldAnswer[] = [],
): AnswerValues {
	const byCode = new Map(answers.map((answer) => [answer.fieldCode, answer.value]));

	return Object.fromEntries(
		fieldsOf(pages).map((field) => [keyOf(field), toControl(field, byCode.get(field.code))]),
	);
}

/** The answers the API takes: every non-empty control, named by the field's code. */
export function toAnswers(pages: readonly FormPage[], values: AnswerValues): FieldAnswer[] {
	return fieldsOf(pages).flatMap((field) => {
		const value = fromControl(field, values[keyOf(field)]);

		return value ? [{ fieldCode: field.code, value }] : [];
	});
}
