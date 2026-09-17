/**
 * Languages the agency actually publishes in - not the full ISO 639-1 list.
 *
 * Keys are upper case because `LanguageCode` normalizes to upper case on the backend and the
 * projections return it that way; a lower case value would match no option here.
 */
export const jobLanguages = {
	PL: "Polish",
	EN: "English",
	DE: "German",
	UA: "Ukrainian",
	CS: "Czech",
	SK: "Slovak",
	ES: "Spanish",
	FR: "French",
	IT: "Italian",
	NL: "Dutch",
};

export function getLanguageLabel(code: string) {
	return (jobLanguages as Record<string, string | undefined>)[code.toUpperCase()] ?? code;
}
