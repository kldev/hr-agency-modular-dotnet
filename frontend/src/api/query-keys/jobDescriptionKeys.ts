export const jobDescriptionKeys = {
	all: ["job-description"] as const,

	lists: () => [...jobDescriptionKeys.all, "list"] as const,

	list: (params?: unknown) => [...jobDescriptionKeys.lists(), params] as const,

	details: (id: string) => [...jobDescriptionKeys.all, "details", id] as const,

	notes: (id: string) => [...jobDescriptionKeys.all, "notes", id] as const,
};
