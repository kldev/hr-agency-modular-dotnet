export interface ScheduleInterviewCommand {
	schedule: (jobApplicationId: string) => void;
}

export interface ChangeInterviewFormatCommand {
	changeFormat: (id: string) => void;
}

export interface ChangeInterviewerCommand {
	changeInterviewer: (id: string) => void;
}

export interface ChangeInterviewTypeCommand {
	changeType: (id: string) => void;
}

export interface ChangeInterviewStatusCommand {
	changeStatus: (id: string) => void;
}
