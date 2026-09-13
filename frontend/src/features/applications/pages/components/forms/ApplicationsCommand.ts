import type { JobApplicationStatus } from "@/api/models";

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
