/*
 * The client half of the upload contract, shared by projects, workers and assignments. The backend
 * checks all of it again - this is here so that a 30 MB file fails the moment the user picks it
 * rather than after a minute of uploading.
 */

export const MAX_DOCUMENT_SIZE_BYTES = 25 * 1024 * 1024;

/** Kept next to the `accept` attribute of the file input and checked again before the upload. */
export const ALLOWED_DOCUMENT_TYPES = [
	"application/pdf",
	"image/jpeg",
	"image/png",
	"application/msword",
	"application/vnd.openxmlformats-officedocument.wordprocessingml.document",
	"application/vnd.ms-excel",
	"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
];

export const DOCUMENT_ACCEPT = ".pdf,.jpg,.jpeg,.png,.doc,.docx,.xls,.xlsx";

/**
 * The file service being down is not a missing document: the API maps it to 503, and that deserves
 * a sentence naming the cause rather than the generic error box.
 */
export function isStorageUnavailable(error: unknown): boolean {
	return (
		typeof error === "object" &&
		error !== null &&
		"response" in error &&
		(error as { response?: { status?: number } }).response?.status === 503
	);
}

export function rejectDocument(file: File): string | null {
	if (file.size > MAX_DOCUMENT_SIZE_BYTES) {
		return "The file is larger than 25 MB.";
	}

	if (!ALLOWED_DOCUMENT_TYPES.includes(file.type)) {
		return "That file type is not accepted.";
	}

	return null;
}
