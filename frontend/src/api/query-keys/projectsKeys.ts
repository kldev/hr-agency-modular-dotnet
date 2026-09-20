export const projectsKeys = {
	all: ["projects"] as const,

	lists: () => [...projectsKeys.all, "list"] as const,

	list: (params?: unknown) => [...projectsKeys.lists(), params] as const,

	details: (id: string) => [...projectsKeys.all, "details", id] as const,

	compliance: (id: string) => [...projectsKeys.all, "compliance", id] as const,
};
