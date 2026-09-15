import type { JobApplicationStatus } from "@/api/models";

export type JobApplicationsActionsType =
	| "add-note"
	| "change-status"
	| "edit"
	| "create"
	| "schedule"
	| "tag";
export interface AddJobApplicationNoteFormCommand {
	addNote(jobApplicationId: string): void;
}
export interface ChangeJobApplicationStatusFormCommand {
	changeStatus(jobApplicationId: string, curent: JobApplicationStatus): void;
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
		display?: string,
	) => void;
}
