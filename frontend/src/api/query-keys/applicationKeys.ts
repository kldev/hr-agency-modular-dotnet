export const applicationKeys = {
	all: ["job-applications"] as const,

	lists: () => [...applicationKeys.all, "list"] as const,

	list: (params?: unknown) => [...applicationKeys.lists(), params] as const,

	details: () => [...applicationKeys.all, "detail"] as const,

	detail: (id: string) => [...applicationKeys.details(), id] as const,

	notes: (id: string) => [...applicationKeys.all, "notes", id] as const,

	timeline: (id: string) => [...applicationKeys.all, "timeline", id] as const,
};
