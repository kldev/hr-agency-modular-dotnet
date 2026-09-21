export const workersKeys = {
	all: ["workers"] as const,

	lists: () => [...workersKeys.all, "list"] as const,

	list: (params?: unknown) => [...workersKeys.lists(), params] as const,

	details: (id: string) => [...workersKeys.all, "details", id] as const,
};
