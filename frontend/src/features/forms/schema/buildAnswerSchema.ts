import { z } from "zod";
import type { FormField, FormPage } from "#/api/models";
import { type AnswerValues, fromControl } from "./answerValues";
import { keyOf } from "./fieldKeys";
import { normalizeValue, type ValidationMode, validateValue } from "./validateValue";

/**
 * A Zod schema for a form that exists only as data. Each key is a field of the layout, and its check
 * is the mirror validator run on the value the control stands for - so the schema says exactly what
 * the backend will say, rather than a second, slightly different reading of the same rules.
 *
 * The mode may differ per page: a wizard holds a page to the full rules once somebody tried to leave
 * it, while pages not reached yet only get the format checks - nobody wants "required" on a page
 * they have not opened.
 *
 * This is the runtime counterpart of `allSchemaFieldsAreGated` in the fixed wizards: every key of the
 * schema is a field of some page by construction, so no field can escape the page validation.
 */
export function buildAnswerSchema(
	pages: readonly FormPage[],
	mode: ValidationMode | ((page: FormPage) => ValidationMode),
) {
	const modeOf = typeof mode === "function" ? mode : () => mode;

	const shape = Object.fromEntries(
		pages.flatMap((page) =>
			page.fields.map((field) => [
				keyOf(field),
				z.any().superRefine((control, context) => {
					const value = normalizeValue(field.type, fromControl(field, control));
					const error = validateValue(field, value, modeOf(page));

					if (error) {
						context.addIssue({ code: "custom", message: error.message });
					}
				}),
			]),
		),
	);

	return z.object(shape);
}

/** The keys of one page, for `form.validateField` and `stepHasErrors` on "Next". */
export function pageKeys(page: FormPage): string[] {
	return page.fields.map(keyOf);
}

export type AnswerProblem = { page: FormPage; field: FormField; message: string };

/**
 * Everything wrong with the answers, named by page and field - computed from the values, not from
 * TanStack's field meta. A wizard mounts only the page on screen, so the meta of every other page is
 * empty exactly when "Submit" needs to know about it.
 */
export function answerProblems(
	pages: readonly FormPage[],
	values: AnswerValues,
	mode: ValidationMode,
): AnswerProblem[] {
	return pages.flatMap((page) =>
		page.fields.flatMap((field) => {
			const value = normalizeValue(field.type, fromControl(field, values[keyOf(field)]));
			const error = validateValue(field, value, mode);

			return error ? [{ page, field, message: error.message }] : [];
		}),
	);
}
