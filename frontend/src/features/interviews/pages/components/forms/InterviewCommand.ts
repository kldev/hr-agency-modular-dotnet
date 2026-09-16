import type { PersonInfo } from "#/types";

export type InterviewActionsType =
	| "change-format"
	| "change-status"
	| "change-Interviewer"
	| "reschedule";

export interface ScheduleInterviewCommand {
	schedule: (jobApplicationId: string, applicant: PersonInfo) => void;
}

export interface ChangeInterviewFormatCommand {
	changeFormat: (id: string) => void;
}

export interface ChangeInterviewerCommand {
	changeInterviewer: (id: string) => void;
}

export interface ChangeInterviewStatusCommand {
	changeStatus: (id: string) => void;
}

export interface RescheduleInterviewCommand {
	reschedule: (id: string) => void;
}

export interface InterviewActionsRef {
	update: (id: string, action: InterviewActionsType) => void;
}
