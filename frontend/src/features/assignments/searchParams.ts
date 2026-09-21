import type { AssignmentStatus } from "@/api/models";

export type AssignmentsSearch = {
	search?: string;
	status?: AssignmentStatus;
	/** Arrive from a worker or a project page; the toolbar never sets them. */
	workerId?: string;
	projectId?: string;
};

export function validateAssignmentsSearch(search: Record<string, unknown>): AssignmentsSearch {
	return {
		search: typeof search.search === "string" ? search.search : undefined,
		status: typeof search.status === "string" ? (search.status as AssignmentStatus) : undefined,
		workerId: typeof search.workerId === "string" ? search.workerId : undefined,
		projectId: typeof search.projectId === "string" ? search.projectId : undefined,
	};
}

export const emptyAssignmentsSearch: AssignmentsSearch = {
	search: undefined,
	status: undefined,
	workerId: undefined,
	projectId: undefined,
};
