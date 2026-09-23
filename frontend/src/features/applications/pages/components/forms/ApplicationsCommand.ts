import type { PersonInfo } from "#/types";
import type { JobApplicationStatus, JobApplicationUpdateStatus } from "@/api/models";

export type JobApplicationsActionsType =
	| "add-note"
	| "change-status"
	| "edit"
	| "create"
	| "schedule"
	| "tag"
	| "register-worker";
export interface AddJobApplicationNoteFormCommand {
	addNote(jobApplicationId: string): void;
}
export interface ChangeJobApplicationStatusFormCommand {
	/** `target` preselects the new status - the column a card was dropped on. */
	changeStatus(
		jobApplicationId: string,
		curent: JobApplicationStatus,
		target?: JobApplicationUpdateStatus,
	): void;
}

export interface EditApplicantCommand {
	edit(jobApplicationId: string): void;
}
export interface CreateJobApplicationsCommand {
	create(jobPostId: string, jobPostTitle: string): void;
}

export interface JobApplicationsRef {
	update: (
		id: string,
		action: JobApplicationsActionsType,
		curent?: JobApplicationStatus,
		info?: PersonInfo,
	) => void;
	changeStatus: ChangeJobApplicationStatusFormCommand["changeStatus"];
}
