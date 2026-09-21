export const assignmentsKeys = {
	all: ["assignments"] as const,

	lists: () => [...assignmentsKeys.all, "list"] as const,

	list: (params?: unknown) => [...assignmentsKeys.lists(), params] as const,

	details: (id: string) => [...assignmentsKeys.all, "details", id] as const,

	compliance: (id: string) => [...assignmentsKeys.all, "compliance", id] as const,
};
