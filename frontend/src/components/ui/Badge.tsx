import clsx from "clsx";
import { applicationSources, applicationStatuses } from "#/features/applications/types";
import { interviewFormats, interviewStatuses, interviewTypes } from "#/features/interviews/type";
import { jobDescriptionStatuses } from "#/features/job-descriptions/type";
import { jobPostsStatuses } from "#/features/job-posts/type";
import {
	complianceStatusClass,
	complianceStatuses,
	contractStatusClass,
	contractStatuses,
	projectStatuses,
} from "#/features/projects/types";
import type {
	CandidateSource,
	ComplianceStatus,
	ContractStatus,
	InterviewFormat,
	InterviewStatus,
	InterviewType,
	JobApplicationStatus,
	JobDescriptionStatus,
	JobPostStatus,
	OpportunityStage,
	ProjectStatus,
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
	return (
		<span className={clsx("badge", applicationsClass[status])}>{applicationStatuses[status]}</span>
	);
}

const jobPostsClass: Record<JobPostStatus, string> = {
	Draft: "badge-new",
	Published: "badge-qualified",
	Closed: "badge-closed",
	Archived: "badge-suspended",
};

export function JobPostsBadge({ status }: { status: JobPostStatus }) {
	return <span className={clsx("badge", jobPostsClass[status])}>{jobPostsStatuses[status]}</span>;
}

const jobDescriptionClass: Record<JobDescriptionStatus, string> = {
	Draft: "badge-new",
	Cancelled: "badge-lost",
	Open: "badge-qualified",
	OnHold: "badge-qualified",
	Closed: "badge-closed",
};

export function JobDescriptionBadge({ status }: { status: JobDescriptionStatus }) {
	return (
		<span className={clsx("badge", jobDescriptionClass[status])}>
			{jobDescriptionStatuses[status]}
		</span>
	);
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
	return <span className={clsx("badge", interviewClass[status])}>{interviewStatuses[status]}</span>;
}

const interviewFormatClasses: Record<InterviewFormat, string> = {
	Online: "badge-new",
	OnSite: "badge-contacted",
	Phone: "badge-qualified",
};

export function InterviewFormatBadge({ format }: { format: InterviewFormat }) {
	return (
		<span className={clsx("badge", interviewFormatClasses[format])}>
			{interviewFormats[format]}
		</span>
	);
}

const interviewType: Record<InterviewType, string> = {
	Hr: "badge-new",
	Final: "badge-contacted",
	Client: "badge-qualified",
	Technical: "badge-contacted",
};

export function InterviewTypeBadge({ status }: { status: InterviewType }) {
	return <span className={clsx("badge", interviewType[status])}>{interviewTypes[status]}</span>;
}

export function CandidateSourceBadge({ source }: { source: CandidateSource }) {
	return <span className="badge badge-contacted">{applicationSources[source]}</span>;
}

const projectClass: Record<ProjectStatus, string> = {
	Draft: "badge-draft",
	Active: "badge-active",
	Suspended: "badge-suspended",
	Completed: "badge-completed",
	Cancelled: "badge-cancelled",
};

export function ProjectStatusBadge({ status }: { status: ProjectStatus }) {
	return <span className={clsx("badge", projectClass[status])}>{projectStatuses[status]}</span>;
}

export function ContractStatusBadge({ status }: { status: ContractStatus }) {
	return (
		<span className={clsx("badge", contractStatusClass[status])}>{contractStatuses[status]}</span>
	);
}

export function ComplianceStatusBadge({ status }: { status: ComplianceStatus }) {
	return (
		<span className={clsx("badge", complianceStatusClass[status])}>
			{complianceStatuses[status]}
		</span>
	);
}
