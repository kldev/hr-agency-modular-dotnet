import type { BadRequestDetails } from "#/api/models";

/**
 * The `fieldErrors` of a 400, keyed as the screen names its fields - field codes for a filled form,
 * page and field ids for the builder. Null when the refusal named no field.
 */
export function fieldErrorsOf(error: unknown): Record<string, string[]> | null {
	return (
		((error as BadRequestDetails | null)?.fieldErrors as Record<string, string[]> | null) ?? null
	);
}
