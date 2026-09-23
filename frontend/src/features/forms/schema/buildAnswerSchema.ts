import { z } from "zod";
import type { FormPage } from "#/api/models";
import { fieldsOf, fromControl } from "./answerValues";
import { keyOf } from "./fieldKeys";
import { normalizeValue, type ValidationMode, validateValue } from "./validateValue";

/**
 * A Zod schema for a form that exists only as data. Each key is a field of the layout, and its check
 * is the mirror validator run on the value the control stands for - so the schema says exactly what
 * the backend will say, rather than a second, slightly different reading of the same rules.
 * <p>
 * This is the runtime counterpart of `allSchemaFieldsAreGated` in the fixed wizards: every key of the
 * schema is a field of some page by construction, so no field can escape the page validation.
 */
export function buildAnswerSchema(pages: readonly FormPage[], mode: ValidationMode) {
	const shape = Object.fromEntries(
		fieldsOf(pages).map((field) => [
			keyOf(field),
			z.any().superRefine((control, context) => {
				const error = validateValue(
					field,
					normalizeValue(field.type, fromControl(field, control)),
					mode,
				);

				if (error) {
					context.addIssue({ code: "custom", message: error.message });
				}
			}),
		]),
	);

	return z.object(shape);
}

/** The keys of one page, for `form.validateField` and `stepHasErrors` on "Next". */
export function pageKeys(page: FormPage): string[] {
	return page.fields.map(keyOf);
}
