import type { EmploymentType, JobDescriptionStatus, WorkMode } from "@/api/models";

export const jobDescriptionStatuses: Record<JobDescriptionStatus, string> = {
	Draft: "Draft",
	Open: "Open",
	OnHold: "OnHold",
	Closed: "Closed",
	Cancelled: "Cancelled",
};

export const employmentTypesOptions: Record<EmploymentType, string> = {
	FullTime: "FullTime",
	PartTime: "PartTime",
	Contract: "Contract",
	Temporary: "Temporary",
	Internship: "Internship",
};

export const workModeOptions: Record<WorkMode, string> = {
	OnSite: "OnSite",
	Hybrid: "Hybrid",
	Remote: "Remote",
};
