export const tasksKeys = {
	all: ["tasks"] as const,

	boards: () => [...tasksKeys.all, "board"] as const,

	board: (params?: unknown) => [...tasksKeys.boards(), params] as const,

	details: (id: string) => [...tasksKeys.all, "details", id] as const,
};
