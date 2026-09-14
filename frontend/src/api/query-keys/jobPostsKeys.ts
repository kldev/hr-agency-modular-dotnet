export const jobPostsKeys = {
	all: ["job-posts"] as const,

	lists: () => [...jobPostsKeys.all, "list"] as const,

	list: (params?: unknown) => [...jobPostsKeys.lists(), params] as const,

	details: (id: string) => [...jobPostsKeys.all, "details", id] as const,
};
