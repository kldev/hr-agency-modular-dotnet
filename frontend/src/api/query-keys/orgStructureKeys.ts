/*
 * The chart is one document per organization, not a collection, so there is no `list`/`detail`
 * pair here: every unit lives inside the same `OrgStructureProjection`.
 *
 * `supervisor` and `subordinates` are nested under `all` on purpose. A move changes who answers for
 * whom, and invalidating `all` then catches those two by prefix without anybody having to list them.
 */
export const orgStructureKeys = {
	all: ["org-structure"] as const,

	chart: () => [...orgStructureKeys.all, "chart"] as const,

	supervisor: (userId: string) => [...orgStructureKeys.all, "supervisor", userId] as const,

	subordinates: (userId: string, wholeSubtree: boolean) =>
		[...orgStructureKeys.all, "subordinates", userId, wholeSubtree] as const,
};
