import type { FieldValue, FormField } from "#/api/models";

/**
 * The TypeScript mirror of `Forms/Domain/Validation/FormAnswersValidator.cs` - same codes, same
 * messages, same order (type, required, then the rules), one error per field. The backend is the
 * authority; this exists so a wrong value is marked while it is typed and "Next" can refuse a page.
 * Both are held to `tests/fixtures/forms-validation-cases.json` - change one, change the other and
 * add a case.
 */

export type ValidationMode = "draft" | "submit";

export type FieldErrorCode =
	| "wrongType"
	| "required"
	| "minLength"
	| "maxLength"
	| "pattern"
	| "email"
	| "phone"
	| "country"
	| "min"
	| "max"
	| "decimals"
	| "minDate"
	| "maxDate"
	| "option"
	| "minSelected"
	| "maxSelected";

export type FieldError = { code: FieldErrorCode; message: string };

type ValidatedField = Pick<FormField, "type" | "rules" | "options">;

export const MAX_TEXT_LENGTH = 10_000;

export const messages = {
	wrongType: "The answer is not of the kind this field takes.",
	required: "This field is required.",
	requiredConsent: "This box has to be ticked.",
	email: "Enter a valid e-mail address.",
	phone: "Enter a valid phone number.",
	country: "Pick a country.",
	option: "Pick one of the listed options.",
	pattern: "The value has the wrong format.",
	minLength: (length: number) => `Enter at least ${length} characters.`,
	maxLength: (length: number) => `Enter at most ${length} characters.`,
	min: (min: number | string) => `Enter a number no smaller than ${min}.`,
	max: (max: number | string) => `Enter a number no greater than ${max}.`,
	decimals: (decimals: number) =>
		decimals === 0 ? "Enter a whole number." : `Use at most ${decimals} decimal places.`,
	minDate: (date: string) => `Pick a date on or after ${date}.`,
	maxDate: (date: string) => `Pick a date on or before ${date}.`,
	minSelected: (count: number) => `Pick at least ${count}.`,
	maxSelected: (count: number) => `Pick at most ${count}.`,
} as const;

type Slot = "text" | "number" | "date" | "boolean" | "values";

const slots: readonly Slot[] = ["text", "number", "date", "boolean", "values"];

export function slotFor(type: FormField["type"]): Slot {
	switch (type) {
		case "Number":
			return "number";
		case "Date":
			return "date";
		case "Boolean":
			return "boolean";
		case "MultiChoice":
			return "values";
		default:
			return "text";
	}
}

/** Mirror of `FieldAnswers.Normalize`: trimmed text, upper-case country, a selection without repeats. */
export function normalizeValue(
	type: FormField["type"],
	value: FieldValue | null | undefined,
): FieldValue | null {
	if (!value) {
		return null;
	}

	const trimmed = typeof value.text === "string" ? value.text.trim() : null;
	const text = trimmed ? (type === "Country" ? trimmed.toUpperCase() : trimmed) : null;
	const values = value.values
		? [...new Set(value.values.map((v) => v.trim()).filter((v) => v.length > 0))]
		: null;

	const normalized: FieldValue = {
		...value,
		text,
		values: values && values.length > 0 ? values : null,
	};

	return isEmpty(normalized) ? null : normalized;
}

export function isEmpty(value: FieldValue | null | undefined): boolean {
	return (
		!value ||
		((value.text ?? "").trim() === "" &&
			value.number == null &&
			value.date == null &&
			value.boolean == null &&
			(value.values == null || value.values.length === 0))
	);
}

export function validateValue(
	field: ValidatedField,
	value: FieldValue | null | undefined,
	mode: ValidationMode,
): FieldError | null {
	const slot = slotFor(field.type);
	const rules = field.rules;

	if (value && slots.some((other) => other !== slot && value[other] != null)) {
		return error(field, "wrongType", messages.wrongType, false);
	}

	if (isEmpty(value) || (field.type === "Boolean" && value?.boolean === false)) {
		if (mode === "submit" && rules.required) {
			return error(
				field,
				"required",
				field.type === "Boolean" ? messages.requiredConsent : messages.required,
			);
		}

		return null;
	}

	const present = value as FieldValue;

	switch (field.type) {
		case "Number":
			return validateNumber(field, Number(present.number));
		case "Date":
			return validateDate(field, present.date as string);
		case "Boolean":
			return null;
		case "SingleChoice":
			return field.options.some((option) => option.value === present.text?.trim())
				? null
				: error(field, "option", messages.option, false);
		case "MultiChoice":
			return validateSelection(field, present.values ?? []);
		case "Country":
			return /^[A-Z]{2}$/.test((present.text ?? "").trim())
				? null
				: error(field, "country", messages.country, false);
		default:
			return validateText(field, (present.text ?? "").trim());
	}
}

/** The generated client types whole numbers as `number | string`; the API always sends numbers. */
function num(value: number | string | null | undefined): number | null {
	return value == null || value === "" ? null : Number(value);
}

function validateText(field: ValidatedField, text: string): FieldError | null {
	const rules = field.rules;
	const min = num(rules.minLength);

	if (min != null && text.length < min) {
		return error(field, "minLength", messages.minLength(min));
	}

	const max = num(rules.maxLength) ?? MAX_TEXT_LENGTH;

	if (text.length > max) {
		return error(field, "maxLength", messages.maxLength(max));
	}

	if (field.type === "Email" && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(text)) {
		return error(field, "email", messages.email);
	}

	if (field.type === "Phone" && !isPhone(text)) {
		return error(field, "phone", messages.phone);
	}

	if (rules.pattern && !matchesWhole(rules.pattern, text)) {
		return error(field, "pattern", messages.pattern);
	}

	return null;
}

function validateNumber(field: ValidatedField, number: number): FieldError | null {
	const rules = field.rules;

	if (rules.min != null && number < Number(rules.min)) {
		return error(field, "min", messages.min(rules.min));
	}

	if (rules.max != null && number > Number(rules.max)) {
		return error(field, "max", messages.max(rules.max));
	}

	const decimals = num(rules.decimals);

	if (decimals != null && decimalPlaces(number) > decimals) {
		return error(field, "decimals", messages.decimals(decimals));
	}

	return null;
}

function validateDate(field: ValidatedField, date: string): FieldError | null {
	const rules = field.rules;

	// ISO dates compare correctly as strings, and never move across a timezone.
	if (rules.minDate && date < rules.minDate) {
		return error(field, "minDate", messages.minDate(rules.minDate));
	}

	if (rules.maxDate && date > rules.maxDate) {
		return error(field, "maxDate", messages.maxDate(rules.maxDate));
	}

	return null;
}

function validateSelection(field: ValidatedField, values: readonly string[]): FieldError | null {
	if (values.some((value) => !field.options.some((option) => option.value === value))) {
		return error(field, "option", messages.option, false);
	}

	const rules = field.rules;

	const min = num(rules.minSelected);
	const max = num(rules.maxSelected);

	if (min != null && values.length < min) {
		return error(field, "minSelected", messages.minSelected(min));
	}

	if (max != null && values.length > max) {
		return error(field, "maxSelected", messages.maxSelected(max));
	}

	return null;
}

/** The pattern has to match the whole value, as on the backend. A broken pattern never matches. */
export function matchesWhole(pattern: string, text: string): boolean {
	try {
		return new RegExp(`^(?:${pattern})$`).test(text);
	} catch {
		return false;
	}
}

export function isValidPattern(pattern: string): boolean {
	try {
		new RegExp(`^(?:${pattern})$`);
		return true;
	} catch {
		return false;
	}
}

function isPhone(text: string): boolean {
	if (!/^\+?[0-9 ()-]+$/.test(text)) {
		return false;
	}

	const digits = text.replace(/\D/g, "").length;

	return digits >= 6 && digits <= 15;
}

function decimalPlaces(number: number): number {
	const [, fraction = ""] = String(number).split(".");

	return fraction.length;
}

function error(
	field: ValidatedField,
	code: FieldErrorCode,
	message: string,
	overridable = true,
): FieldError {
	const own = field.rules.message?.trim();

	return { code, message: overridable && own ? own : message };
}
