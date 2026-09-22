import type { CandidateSource, JobApplicationStatus } from "@/api/models";

export const applicationStatuses: Record<JobApplicationStatus, string> = {
	Applied: "Applied",
	Screening: "Screening",
	Interview: "Interview",
	Assessment: "Assessment",
	Offer: "Offer",
	Hired: "Hired",
	Rejected: "Rejected",
	Withdrawn: "Withdrawn",
};

export const applicationSources: Record<CandidateSource, string> = {
	CareerPage: "Career Page",
	PracujPl: "Pracuj PL",
	Olx: "Olx",
	PracaPl: "Praca PL",
	RocketJobs: "Rocket Jobs",
	JustJoinIt: "JustJoinIt",
	NoFluffJobs: "NoFluffJobs",
	LinkedIn: "LinkedIn",
	Indeed: "Indeed",
	Referral: "Referral",
	DirectSourcing: "Direct sourcing",
	InternalDatabase: "Internal database",
	RecruitmentAgency: "Recruitment Agency",
	DirectApplication: "Direct Application",
	Facebook: "Facebook",
	Other: "Other",
	Direct: "Direct",
};

/**
 * The "is there a workers' file" filter, shared by the applications and the candidates list. A
 * select rather than a toggle, because "not yet" is as much a question as "already" - it is the
 * list somebody works through.
 */
export type WorkerFileFilter = "registered" | "not-registered";

export const workerFileFilters: Record<WorkerFileFilter, string> = {
	registered: "In workers' register",
	"not-registered": "Not in workers' register",
};

/** The API's `registeredAsWorker`: undefined leaves the list alone. */
export function registeredAsWorker(filter: WorkerFileFilter | undefined): boolean | undefined {
	return filter === undefined ? undefined : filter === "registered";
}
