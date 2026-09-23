import type { CandidateSource, JobApplicationStatus } from "@/api/models";
import { parseViewMode, type ViewMode } from "@/components/ui";
import { type WorkerFileFilter, workerFileFilters } from "./types";

export interface ApplicationFilters {
	status?: JobApplicationStatus;
	source?: CandidateSource;
	search?: string;
	worker?: WorkerFileFilter;
}

export type ApplicationsSearch = ApplicationFilters & {
	view?: ViewMode;
};

export function validateApplicationsSearch(search: Record<string, unknown>): ApplicationsSearch {
	return {
		status: typeof search.status === "string" ? (search.status as JobApplicationStatus) : undefined,
		source: typeof search.source === "string" ? (search.source as CandidateSource) : undefined,
		search: typeof search.search === "string" ? search.search : undefined,
		worker:
			typeof search.worker === "string" && search.worker in workerFileFilters
				? (search.worker as WorkerFileFilter)
				: undefined,
		view: parseViewMode(search.view),
	};
}
