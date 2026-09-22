/*
 * The register is one record per person, so `details` is keyed by the user rather than by a record
 * id - that is also the only id the endpoints take.
 */
export const agencyEmploymentKeys = {
	all: ["agency-employment"] as const,

	lists: () => [...agencyEmploymentKeys.all, "list"] as const,

	list: (params?: unknown) => [...agencyEmploymentKeys.lists(), params] as const,

	details: (userId: string) => [...agencyEmploymentKeys.all, "details", userId] as const,
};
