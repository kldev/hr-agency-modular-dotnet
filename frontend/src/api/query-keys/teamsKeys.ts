export const teamsKeys = {
	all: ["teams"] as const,

	lists: () => [...teamsKeys.all, "list"] as const,

	list: (params?: unknown) => [...teamsKeys.all, "list", params] as const,

	detail: (id: string) => [...teamsKeys.all, "details", id] as const,
};
