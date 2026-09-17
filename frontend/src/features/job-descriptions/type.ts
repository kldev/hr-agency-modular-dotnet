import type { EmploymentType, JobDescriptionStatus, WorkMode } from "@/api/models";

export const jobDescriptionStatuses: Record<JobDescriptionStatus, string> = {
	Draft: "Draft",
	Open: "Open",
	OnHold: "OnHold",
	Closed: "Closed",
	Cancelled: "Cancelled",
};

export const employmentTypesOptions: Record<EmploymentType, string> = {
	FullTime: "Full time",
	PartTime: "Part time",
	Contract: "Contract",
	Temporary: "Temporary",
	Internship: "Internship",
};

export const employmentTypeDescriptions: Partial<Record<EmploymentType, string>> = {
	FullTime: "Standard full-time employment.",
	PartTime: "Reduced working schedule.",
	Contract: "Contract or B2B engagement.",
	Temporary: "Time-limited engagement.",
	Internship: "Intern or graduate placement.",
};

export const workModeOptions: Record<WorkMode, string> = {
	OnSite: "On-site",
	Hybrid: "Hybrid",
	Remote: "Remote",
};

export const workModeDescriptions: Partial<Record<WorkMode, string>> = {
	OnSite: "Work primarily from the company location.",
	Hybrid: "Combine office and remote work.",
	Remote: "Work fully remotely.",
};
