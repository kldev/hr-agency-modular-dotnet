/**
 * Bytes as a person would read them. Shared because both the size shown next to a chosen file and
 * the "this one is too large" message have to phrase the same number the same way.
 */
export function formatFileSize(bytes: number) {
	if (bytes < 1024) return `${bytes} B`;
	if (bytes < 1024 * 1024) return `${Math.round(bytes / 1024)} KB`;

	return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}
