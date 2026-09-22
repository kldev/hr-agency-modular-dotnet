import type { TimeSheetProjection, WorkDay } from "@/api/models";

/** Which day is being written on. The date is the key of the entry, never a field in the form. */
export type WorkDayTarget = {
	date: string;
	day: WorkDay | null;
};

export interface SaveWorkDayFormCommand {
	saveDay: (target: WorkDayTarget) => void;
}

export interface ApproveTimeSheetFormCommand {
	approve: (sheet: TimeSheetProjection) => void;
}

export interface ReturnTimeSheetFormCommand {
	returnForCorrection: (sheet: TimeSheetProjection) => void;
}

export interface CommentOnTimeSheetFormCommand {
	comment: (sheet: TimeSheetProjection) => void;
}

/** Takes the month rather than a sheet: this is the one action reachable before a sheet is read. */
export interface SubmitTimeSheetFormCommand {
	submit: (target: { year: number; month: number; totalMinutes: number; days: number }) => void;
}
