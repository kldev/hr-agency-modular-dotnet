import type { WorkerStatus } from "@/api/models";
import { parseViewMode, type ViewMode } from "@/components/ui";

/**
 * Shared by the two register routes, which render the same page over a different half of it. The
 * work country is not in here: it is what distinguishes the two routes, not something the toolbar
 * changes.
 */
export type WorkersSearch = {
	search?: string;
	status?: WorkerStatus;
	citizenship?: string;
	view?: ViewMode;
};

export function validateWorkersSearch(search: Record<string, unknown>): WorkersSearch {
	return {
		search: typeof search.search === "string" ? search.search : undefined,
		status: typeof search.status === "string" ? (search.status as WorkerStatus) : undefined,
		citizenship: typeof search.citizenship === "string" ? search.citizenship : undefined,
		view: parseViewMode(search.view),
	};
}

export const emptyWorkersSearch: WorkersSearch = {
	search: undefined,
	status: undefined,
	citizenship: undefined,
	view: undefined,
};
