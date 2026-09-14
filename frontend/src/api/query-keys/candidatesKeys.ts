export const candidatesKeys = {
	all: ["candidates"] as const,

	lists: () => [...candidatesKeys.all, "list"] as const,

	list: (params?: unknown) => [...candidatesKeys.lists(), params] as const,

	detail: (id: string) => [...candidatesKeys.all, "details", id] as const,

	notes: (id: string) => [...candidatesKeys.all, "notes", id] as const,
};
