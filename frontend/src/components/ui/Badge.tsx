import clsx from "clsx";
import type { JobApplicationStatus, OpportunityStage } from "@/api/models";

const opportunityClass: Record<OpportunityStage, string> = {
	New: "badge-new",
	Viewed: "badge-viewed",
	Contacted: "badge-contacted",
	Qualified: "badge-qualified",
	Proposal: "badge-proposal",
	Won: "badge-won",
	Lost: "badge-lost",
};

export function OpportunityStageBadge({ status }: { status: OpportunityStage }) {
	return <span className={clsx("badge", opportunityClass[status])}>{status}</span>;
}

const applicationsClass: Record<JobApplicationStatus, string> = {
	Applied: "badge-new",
	Screening: "badge-viewed",
	Interview: "badge-contacted",
	Assessment: "badge-qualified",
	Offer: "badge-proposal",
	Hired: "badge-won",
	Rejected: "badge-lost",
	Withdrawn: "badge-suspended",
};

export function ApplicationBadge({ status }: { status: JobApplicationStatus }) {
	return <span className={clsx("badge", applicationsClass[status])}>{status}</span>;
}
