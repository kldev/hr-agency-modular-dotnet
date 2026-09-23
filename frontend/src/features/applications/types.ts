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

const finalStatuses: JobApplicationStatus[] = ["Hired", "Rejected", "Withdrawn"];

/** Mirrors `JobApplicationStatusChangePolicy.Allow`. */
function allowApplicationStatusChange(
	current: JobApplicationStatus,
	next: JobApplicationStatus,
): boolean {
	if (current === next) return false;

	if ((next === "Rejected" || next === "Withdrawn") && finalStatuses.includes(current)) {
		return false;
	}

	if (next === "Hired" || next === "Rejected" || next === "Withdrawn") return true;

	switch (current) {
		case "Applied":
			return next === "Screening";
		case "Screening":
			return next === "Assessment" || next === "Interview";
		case "Assessment":
			return next === "Interview" || next === "Offer";
		case "Interview":
			return next === "Assessment" || next === "Offer";
		default:
			return false;
	}
}

/**
 * Where an application may go from `status`. `Interview` is on the list, but it is reached by
 * scheduling one - the status change needs the interview's id, which only scheduling produces.
 */
export function allowedApplicationStatusTransitions(
	status: JobApplicationStatus,
): JobApplicationStatus[] {
	return (Object.keys(applicationStatuses) as JobApplicationStatus[]).filter((next) =>
		allowApplicationStatusChange(status, next),
	);
}

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
