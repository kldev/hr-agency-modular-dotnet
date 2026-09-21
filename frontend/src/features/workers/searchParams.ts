import type { ResponsibleDepartment, WorkerStatus } from "@/api/models";

/**
 * Shared by the two register routes, which render the same page over a different half of it. The
 * work country is not in here: it is what distinguishes the two routes, not something the toolbar
 * changes.
 */
export type WorkersSearch = {
	search?: string;
	status?: WorkerStatus;
	department?: ResponsibleDepartment;
	citizenship?: string;
};

export function validateWorkersSearch(search: Record<string, unknown>): WorkersSearch {
	return {
		search: typeof search.search === "string" ? search.search : undefined,
		status: typeof search.status === "string" ? (search.status as WorkerStatus) : undefined,
		department:
			typeof search.department === "string"
				? (search.department as ResponsibleDepartment)
				: undefined,
		citizenship: typeof search.citizenship === "string" ? search.citizenship : undefined,
	};
}

export const emptyWorkersSearch: WorkersSearch = {
	search: undefined,
	status: undefined,
	department: undefined,
	citizenship: undefined,
};
