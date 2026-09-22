import clsx from "clsx";
import { applicationSources, applicationStatuses } from "#/features/applications/types";
import { assignmentStatusClass, assignmentStatuses } from "#/features/assignments/types";
import { complianceStatusClass, complianceStatuses } from "#/features/compliance/types";
import { contractRequiresTimeRecord, workerContractTypes } from "#/features/contracts/types";
import { interviewFormats, interviewStatuses, interviewTypes } from "#/features/interviews/type";
import { jobDescriptionStatuses } from "#/features/job-descriptions/type";
import { jobPostsStatuses } from "#/features/job-posts/type";
import { contractStatusClass, contractStatuses, projectStatuses } from "#/features/projects/types";
import { timeSheetStatusClass, timeSheetStatuses } from "#/features/timesheets/types";
import { workerStatusClass, workerStatuses } from "#/features/workers/types";
import type {
	AssignmentStatus,
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
	TimeSheetStatus,
	WorkerContractType,
	WorkerStatus,
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

/**
 * Trading or not, worked out from the dates rather than a stored flag - the same rule the backend
 * applies, so the badge cannot disagree with what the list filter did.
 */
export function LegalEntityStatusBadge({ isTrading }: { isTrading: boolean }) {
	return (
		<span className={clsx("badge", isTrading ? "badge-active" : "badge-closed")}>
			{isTrading ? "Trading" : "Closed"}
		</span>
	);
}

export function ComplianceStatusBadge({ status }: { status: ComplianceStatus }) {
	return (
		<span className={clsx("badge", complianceStatusClass[status])}>
			{complianceStatuses[status]}
		</span>
	);
}

export function WorkerStatusBadge({ status }: { status: WorkerStatus }) {
	return <span className={clsx("badge", workerStatusClass[status])}>{workerStatuses[status]}</span>;
}

export function AssignmentStatusBadge({ status }: { status: AssignmentStatus }) {
	return (
		<span className={clsx("badge", assignmentStatusClass[status])}>
			{assignmentStatuses[status]}
		</span>
	);
}

export function TimeSheetStatusBadge({ status }: { status: TimeSheetStatus }) {
	return (
		<span className={clsx("badge", timeSheetStatusClass[status])}>{timeSheetStatuses[status]}</span>
	);
}

/**
 * A month nobody has opened is not a zero month, and the monitoring screen exists for exactly this
 * row - so it says so in words rather than showing an empty status cell.
 */
export function TimeSheetStatusOrNotStartedBadge({ status }: { status: TimeSheetStatus | null }) {
	if (!status) return <span className="badge badge-inactive">Not started</span>;

	return <TimeSheetStatusBadge status={status} />;
}

/** Whether this contract carries the duty to record hours - derived, exactly as the backend has it. */
export function ContractTypeBadge({ contractType }: { contractType: WorkerContractType }) {
	return (
		<span
			className={clsx(
				"badge",
				contractRequiresTimeRecord[contractType] ? "badge-active" : "badge-inactive",
			)}
			title={
				contractRequiresTimeRecord[contractType]
					? "Covered by the duty to record hours"
					: "No duty to record hours"
			}
		>
			{workerContractTypes[contractType]}
		</span>
	);
}
