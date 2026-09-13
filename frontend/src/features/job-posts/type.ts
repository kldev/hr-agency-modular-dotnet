import type { JobPostStatus, PostingChannelType } from "@/api/models";

export const jobPostsStatuses: Record<JobPostStatus, string> = {
	Draft: "Draft",
	Published: "Published",
	Closed: "Closed",
	Archived: "Archived",
};

export const jobPostChannels: Record<PostingChannelType, string> = {
	CareerPage: "CareerPage",
	PracujPl: "Pracuj PL",
	Olx: "Olx",
	PracaPl: "Praca PL",
	Rocketjobs: "Rocketjobs",
	JustJoinIt: "JustJoinIt",
	NoFluffJobs: "NoFluffJobs",
	Linkedin: "Linkedin",
	Indeed: "Indeed",
	Other: "Other",
};
