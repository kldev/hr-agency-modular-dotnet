/*
 * Reports are read-only and computed elsewhere; the period is the whole identity of an answer.
 */
export const reportsKeys = {
	all: ["reports"] as const,

	recruitment: (from: string, to: string) => [...reportsKeys.all, "recruitment", from, to] as const,

	platform: (from: string, to: string) => [...reportsKeys.all, "platform", from, to] as const,
};
