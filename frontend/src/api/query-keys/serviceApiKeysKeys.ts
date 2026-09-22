export const serviceApiKeysKeys = {
	all: ["service-api-keys"] as const,

	list: () => [...serviceApiKeysKeys.all, "list"] as const,
};
