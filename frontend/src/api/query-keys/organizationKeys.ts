export const organizationKeys = {
	all: ["organizations"] as const,

	lists: () => [...organizationKeys.all, "list"] as const,

	list: (params?: unknown) => [...organizationKeys.lists(), params] as const,
};
