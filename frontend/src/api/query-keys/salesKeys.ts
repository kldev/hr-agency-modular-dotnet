export const salesKeys = {
	all: ["sales"] as const,

	lists: () => [...salesKeys.all, "list"] as const,

	list: (params?: unknown) => [...salesKeys.lists(), params] as const,

	opportunity: (id: string) => [...salesKeys.all, "opportunity", id] as const,

	totals: (params?: unknown) => [...salesKeys.all, "totals", params] as const,

	activities: (opportunityId: string) => [...salesKeys.all, "activities", opportunityId] as const,

	companyActivities: (companyId: string) =>
		[...salesKeys.all, "company-activities", companyId] as const,
};
