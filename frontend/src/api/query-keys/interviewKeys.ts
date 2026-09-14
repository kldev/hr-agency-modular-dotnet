export const interviewsKeys = {
	all: ["interviews"] as const,

	lists: () => [...interviewsKeys.all, "list"] as const,

	list: (params?: unknown) => [...interviewsKeys.lists(), params] as const,

	details: (id: string) => [...interviewsKeys.all, "details", id] as const,

	range: (params?: unknown) => [...interviewsKeys.all, "range", params] as const,
};
