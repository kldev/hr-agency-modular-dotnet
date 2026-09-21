export const positionsKeys = {
	all: ["positions"] as const,

	list: (filter: unknown) => [...positionsKeys.all, "list", filter] as const,

	details: (id: string) => [...positionsKeys.all, "details", id] as const,
};
