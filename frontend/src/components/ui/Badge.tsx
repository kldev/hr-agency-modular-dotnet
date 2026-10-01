import { Chip } from "@heroui/react";
import clsx from "clsx";
import { applicationSources, applicationStatuses } from "#/features/applications/types";
import { assignmentStatusClass, assignmentStatuses } from "#/features/assignments/types";
import { complianceStatusClass, complianceStatuses } from "#/features/compliance/types";
import { contractRequiresTimeRecord, workerContractTypes } from "#/features/contracts/types";
import {
	formStatusClass,
	formStatuses,
	responseStatusClass,
	responseStatuses,
} from "#/features/forms/types";
import { interviewFormats, interviewStatuses, interviewTypes } from "#/features/interviews/type";
import { jobDescriptionStatuses } from "#/features/job-descriptions/type";
import { jobPostsStatuses } from "#/features/job-posts/type";
import { contractStatusClass, contractStatuses, projectStatuses } from "#/features/projects/types";
import { taskPriorities, taskPriorityClass } from "#/features/tasks/types";
import { timeSheetStatusClass, timeSheetStatuses } from "#/features/timesheets/types";
import { workerStatusClass, workerStatuses } from "#/features/workers/types";
import type {
	AssignmentStatus,
	CandidateSource,
	CompanyStatus,
	ComplianceStatus,
	ContractStatus,
	FormResponseStatus,
	FormStatus,
	InterviewFormat,
	InterviewStatus,
	InterviewType,
	JobApplicationStatus,
	JobDescriptionStatus,
	JobPostStatus,
	OpportunityStage,
	ProjectStatus,
	TaskPriority,
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
	return (
		<Chip size="sm" className={clsx("badge", opportunityClass[status])}>
			{status}
		</Chip>
	);
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
		<Chip size="sm" className={clsx("badge", applicationsClass[status])}>
			{applicationStatuses[status]}
		</Chip>
	);
}

const jobPostsClass: Record<JobPostStatus, string> = {
	Draft: "badge-new",
	Published: "badge-qualified",
	Closed: "badge-closed",
	Archived: "badge-suspended",
};

export function JobPostsBadge({ status }: { status: JobPostStatus }) {
	return (
		<Chip size="sm" className={clsx("badge", jobPostsClass[status])}>
			{jobPostsStatuses[status]}
		</Chip>
	);
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
		<Chip size="sm" className={clsx("badge", jobDescriptionClass[status])}>
			{jobDescriptionStatuses[status]}
		</Chip>
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
	return (
		<Chip size="sm" className={clsx("badge", interviewClass[status])}>
			{interviewStatuses[status]}
		</Chip>
	);
}

const interviewFormatClasses: Record<InterviewFormat, string> = {
	Online: "badge-new",
	OnSite: "badge-contacted",
	Phone: "badge-qualified",
};

export function InterviewFormatBadge({ format }: { format: InterviewFormat }) {
	return (
		<Chip size="sm" className={clsx("badge", interviewFormatClasses[format])}>
			{interviewFormats[format]}
		</Chip>
	);
}

const interviewType: Record<InterviewType, string> = {
	Hr: "badge-new",
	Final: "badge-contacted",
	Client: "badge-qualified",
	Technical: "badge-contacted",
};

export function InterviewTypeBadge({ status }: { status: InterviewType }) {
	return (
		<Chip size="sm" className={clsx("badge", interviewType[status])}>
			{interviewTypes[status]}
		</Chip>
	);
}

export function CandidateSourceBadge({ source }: { source: CandidateSource }) {
	return (
		<Chip size="sm" className="badge badge-contacted">
			{applicationSources[source]}
		</Chip>
	);
}

const projectClass: Record<ProjectStatus, string> = {
	Draft: "badge-draft",
	Active: "badge-active",
	Suspended: "badge-suspended",
	Completed: "badge-completed",
	Cancelled: "badge-cancelled",
};

export function ProjectStatusBadge({ status }: { status: ProjectStatus }) {
	return (
		<Chip size="sm" className={clsx("badge", projectClass[status])}>
			{projectStatuses[status]}
		</Chip>
	);
}

export function ContractStatusBadge({ status }: { status: ContractStatus }) {
	return (
		<Chip size="sm" className={clsx("badge", contractStatusClass[status])}>
			{contractStatuses[status]}
		</Chip>
	);
}

/**
 * Trading or not, worked out from the dates rather than a stored flag - the same rule the backend
 * applies, so the badge cannot disagree with what the list filter did.
 */
export function LegalEntityStatusBadge({ isTrading }: { isTrading: boolean }) {
	return (
		<Chip size="sm" className={clsx("badge", isTrading ? "badge-active" : "badge-closed")}>
			{isTrading ? "Trading" : "Closed"}
		</Chip>
	);
}

export function ComplianceStatusBadge({ status }: { status: ComplianceStatus }) {
	return (
		<Chip size="sm" className={clsx("badge", complianceStatusClass[status])}>
			{complianceStatuses[status]}
		</Chip>
	);
}

export function WorkerStatusBadge({ status }: { status: WorkerStatus }) {
	return (
		<Chip size="sm" className={clsx("badge", workerStatusClass[status])}>
			{workerStatuses[status]}
		</Chip>
	);
}

export function AssignmentStatusBadge({ status }: { status: AssignmentStatus }) {
	return (
		<Chip size="sm" className={clsx("badge", assignmentStatusClass[status])}>
			{assignmentStatuses[status]}
		</Chip>
	);
}

export function TimeSheetStatusBadge({ status }: { status: TimeSheetStatus }) {
	return (
		<Chip size="sm" className={clsx("badge", timeSheetStatusClass[status])}>
			{timeSheetStatuses[status]}
		</Chip>
	);
}

/**
 * A month nobody has opened is not a zero month, and the monitoring screen exists for exactly this
 * row - so it says so in words rather than showing an empty status cell.
 */
export function TimeSheetStatusOrNotStartedBadge({ status }: { status: TimeSheetStatus | null }) {
	if (!status)
		return (
			<Chip size="sm" className="badge badge-inactive">
				Not started
			</Chip>
		);

	return <TimeSheetStatusBadge status={status} />;
}

/** Whether this contract carries the duty to record hours - derived, exactly as the backend has it. */
export function ContractTypeBadge({ contractType }: { contractType: WorkerContractType }) {
	return (
		<Chip
			size="sm"
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
		</Chip>
	);
}

export function FormStatusBadge({ status }: { status: FormStatus }) {
	return (
		<Chip size="sm" className={clsx("badge", formStatusClass[status])}>
			{formStatuses[status]}
		</Chip>
	);
}

export function FormResponseStatusBadge({ status }: { status: FormResponseStatus }) {
	return (
		<Chip size="sm" className={clsx("badge", responseStatusClass[status])}>
			{responseStatuses[status]}
		</Chip>
	);
}

export function TaskPriorityBadge({ priority }: { priority: TaskPriority }) {
	return (
		<Chip size="sm" className={clsx("badge", taskPriorityClass[priority])}>
			{taskPriorities[priority]}
		</Chip>
	);
}

export function CompanyStatusBadge({ status }: { status: CompanyStatus }) {
	return (
		<Chip
			size="sm"
			className={clsx("badge", status === "Active" ? "badge-active" : "badge-inactive")}
		>
			{status}
		</Chip>
	);
}
