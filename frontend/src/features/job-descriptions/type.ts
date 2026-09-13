import type { JobDescriptionStatus } from "@/api/models";

export const jobDescriptionStatuses: Record<JobDescriptionStatus, string> = {
	Draft: "Draft",
	Open: "Open",
	OnHold: "OnHold",
	Closed: "Closed",
	Cancelled: "Cancelled",
};
