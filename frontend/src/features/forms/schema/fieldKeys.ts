import type { FormField } from "#/api/models";

/**
 * The name a field has inside the TanStack form. Never the field's code: a code is dotted
 * (`gdpr.consent`) and TanStack reads a dot as a path into a nested object, so `employee.firstName`
 * and `employee.lastName` would become one `employee` object. The field id is unique in a layout and
 * has no dots.
 */
export function keyOf(field: Pick<FormField, "fieldId">): string {
	return `f_${field.fieldId.replaceAll("-", "")}`;
}
