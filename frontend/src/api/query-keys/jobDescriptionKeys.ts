export const jobDescriptionKeys = {
	all: ["job-description"] as const,

	lists: () => [...jobDescriptionKeys.all, "list"] as const,

	list: (params?: unknown) => [...jobDescriptionKeys.lists(), params] as const,

	details: () => [...jobDescriptionKeys.all, "detail"] as const,

	detail: (id: string) => [...jobDescriptionKeys.details(), id] as const,
};
