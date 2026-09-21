export const suggestionKeys = {
	all: ["suggestion"] as const,

	company: (id: string) => [...suggestionKeys.all, "company", id] as const,

	user: (id: string) => [...suggestionKeys.all, "user", id] as const,

	team: (id: string) => [...suggestionKeys.all, "team", id] as const,

	worker: (id: string) => [...suggestionKeys.all, "worker", id] as const,

	project: (id: string) => [...suggestionKeys.all, "project", id] as const,

	position: (id: string) => [...suggestionKeys.all, "position", id] as const,
};
