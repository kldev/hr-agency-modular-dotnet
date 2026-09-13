import clsx from "clsx";
import type {
	InterviewFormat,
	InterviewStatus,
	InterviewType,
	JobApplicationStatus,
	JobDescriptionStatus,
	JobPostStatus,
	OpportunityStage,
} from "@/api/models";

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

const jobPostsClass: Record<JobPostStatus, string> = {
	Draft: "badge-new",
	Published: "badge-qualified",
	Closed: "badge-closed",
	Archived: "badge-suspended",
};

export function JobPostsBadge({ status }: { status: JobPostStatus }) {
	return <span className={clsx("badge", jobPostsClass[status])}>{status}</span>;
}

const jobDescriptionClass: Record<JobDescriptionStatus, string> = {
	Draft: "badge-new",
	Cancelled: "badge-lost",
	Open: "badge-qualified",
	OnHold: "badge-qualified",
	Closed: "badge-closed",
};

export function JobDescriptionBadge({ status }: { status: JobDescriptionStatus }) {
	return <span className={clsx("badge", jobDescriptionClass[status])}>{status}</span>;
}

const interviewClass: Record<InterviewStatus, string> = {
	Planned: "badge-new",
	Confirmed: "badge-contacted",
	InProgress: "badge-active",
	Rescheduled: "badge-qualified",
	NoShow: "badge-inactive ",
	Canceled: "badge-suspended",
	Completed: "badge-closed",
};

export function InterviewStatusBadge({ status }: { status: InterviewStatus }) {
	return <span className={clsx("badge", interviewClass[status])}>{status}</span>;
}

const interviewFormat: Record<InterviewFormat, string> = {
	Online: "badge-new",
	OnSite: "badge-contacted",
	Phone: "badge-qualified",
};

export function InterviewFormatBadge({ status }: { status: InterviewFormat }) {
	return <span className={clsx("badge", interviewFormat[status])}>{status}</span>;
}

const interviewType: Record<InterviewType, string> = {
	Hr: "badge-new",
	Final: "badge-contacted",
	Client: "badge-qualified",
	Technical: "badge-contacted",
};

export function InterviewTypeBadge({ status }: { status: InterviewType }) {
	return <span className={clsx("badge", interviewType[status])}>{status}</span>;
}
