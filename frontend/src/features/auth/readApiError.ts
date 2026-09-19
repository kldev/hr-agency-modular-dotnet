import type { BadRequestDetails } from "@/api/models";

/**
 * The axios mutator rejects a 400 with the ProblemDetails body, so a business rule message
 * ("The password reset link is invalid or has expired.") arrives as `detail`. Anything else is a
 * transport or server failure and gets the caller's generic wording.
 */
export function readApiError(error: unknown, fallback: string): string {
	const details = error as BadRequestDetails | null;

	if (details?.detail) {
		return details.detail;
	}

	if (details?.validationErrors?.length) {
		return details.validationErrors[0];
	}

	return fallback;
}
