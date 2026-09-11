import type { CandidateSource, JobApplicationStatus } from "@/api/models";

export const applicationStatuses: Record<JobApplicationStatus, string> = {
	Applied: "Applied",
	Screening: "Screening",
	Interview: "Interview",
	Assessment: "Assessment",
	Offer: "Offer",
	Hired: "Hired",
	Rejected: "Rejected",
	Withdrawn: "Withdrawn",
};

export const applicationSources: Record<CandidateSource, string> = {
	CareerPage: "Career Page",
	PracujPl: "Pracuj PL",
	Olx: "Olx",
	PracaPl: "Praca PL",
	RocketJobs: "Rocket Jobs",
	JustJoinIt: "JustJoinIt",
	NoFluffJobs: "NoFluffJobs",
	LinkedIn: "LinkedIn",
	Indeed: "Indeed",
	Referral: "Referral",
	DirectSourcing: "Direct sourcing",
	InternalDatabase: "Internal database",
	RecruitmentAgency: "Recruitment Agency",
	DirectApplication: "Direct Application",
	Facebook: "Facebook",
	Other: "Other",
	Direct: "Direct",
};
