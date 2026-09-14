export const companiesKeys = {
	all: ["companies"] as const,

	lists: () => [...companiesKeys.all, "list"] as const,

	list: (params?: unknown) => [...companiesKeys.lists(), params] as const,

	details: (id: string) => [...companiesKeys.all, "details", id] as const,

	contacts: (id: string) => [...companiesKeys.all, "contacts", id] as const,
};
