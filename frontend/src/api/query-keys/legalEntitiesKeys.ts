export const legalEntitiesKeys = {
	all: ["legal-entities"] as const,

	lists: () => [...legalEntitiesKeys.all, "list"] as const,

	list: (params?: unknown) => [...legalEntitiesKeys.lists(), params] as const,

	detail: (id: string) => [...legalEntitiesKeys.all, "details", id] as const,
};
