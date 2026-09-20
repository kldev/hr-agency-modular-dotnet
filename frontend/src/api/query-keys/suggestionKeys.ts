export const suggestionKeys = {
	all: ["suggestion"] as const,

	company: (id: string) => [...suggestionKeys.all, "company", id] as const,

	user: (id: string) => [...suggestionKeys.all, "user", id] as const,

	team: (id: string) => [...suggestionKeys.all, "team", id] as const,
};
