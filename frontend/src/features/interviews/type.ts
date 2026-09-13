import type { InterviewFormat, InterviewStatus, InterviewType } from "@/api/models";

export const interviewStatuses: Record<InterviewStatus, string> = {
	Planned: "Planned",
	Confirmed: "Confirmed",
	InProgress: "In progress",
	Completed: "Completed",
	Canceled: "Canceled",
	NoShow: "No show",
	Rescheduled: "Rescheduled",
};

export const interviewTypes: Record<InterviewType, string> = {
	Hr: "Hr",
	Technical: "Technical",
	Client: "Client",
	Final: "Final",
};

export const interviewFormats: Record<InterviewFormat, string> = {
	Online: "Online",
	OnSite: "OnSite",
	Phone: "Phone",
};
