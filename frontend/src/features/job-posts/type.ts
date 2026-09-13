import type { JobPostStatus } from "@/api/models";

export const jobPostsStatuses: Record<JobPostStatus, string> = {
	Draft: "Draft",
	Published: "Published",
	Closed: "Closed",
	Archived: "Archived",
};
