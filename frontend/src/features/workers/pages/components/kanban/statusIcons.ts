import {
	ArrowLeftRight,
	BriefcaseBusiness,
	FilePen,
	GraduationCap,
	type LucideIcon,
	Stamp,
	UserSearch,
	UserX,
} from "lucide-react";
import type { WorkerStatus } from "#/api/models";

export const workerStatusIcons: Record<WorkerStatus, LucideIcon> = {
	Recruitment: UserSearch,
	ContractPreparation: FilePen,
	Legalisation: Stamp,
	Onboarding: GraduationCap,
	Employed: BriefcaseBusiness,
	ProjectChange: ArrowLeftRight,
	Terminated: UserX,
};

/** The pipeline in the order it is walked, which is the order of the columns. */
export const workerPipeline = Object.keys(workerStatusIcons) as WorkerStatus[];
