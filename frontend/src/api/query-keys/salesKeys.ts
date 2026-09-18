export const salesKeys = {
	all: ["sales"] as const,

	lists: () => [...salesKeys.all, "list"] as const,

	list: (params?: unknown) => [...salesKeys.lists(), params] as const,

	opportunity: (id: string) => [...salesKeys.all, "opportunity", id] as const,

	activities: (opportunityId: string) => [...salesKeys.all, "activities", opportunityId] as const,
};
