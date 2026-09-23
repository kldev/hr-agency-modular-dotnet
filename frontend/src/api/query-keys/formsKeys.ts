export const formsKeys = {
	all: ["forms"] as const,

	lists: () => [...formsKeys.all, "list"] as const,

	list: (params?: unknown) => [...formsKeys.lists(), params] as const,

	details: (id: string) => [...formsKeys.all, "details", id] as const,

	version: (id: string, version: number) => [...formsKeys.all, "version", id, version] as const,

	systemFields: (includeArchived: boolean) =>
		[...formsKeys.all, "system-fields", includeArchived] as const,
};

/** Kept apart from `formsKeys`: filling a form in never has to refetch the builder, and back. */
export const formResponsesKeys = {
	all: ["form-responses"] as const,

	forSubject: (kind: string, id: string) =>
		[...formResponsesKeys.all, "subject", kind, id] as const,

	available: (kind: string, id: string) =>
		[...formResponsesKeys.all, "available", kind, id] as const,

	details: (id: string) => [...formResponsesKeys.all, "details", id] as const,
};
