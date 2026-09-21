import type { AssignmentDocumentCategory, AssignmentStatus } from "@/api/models";

export const assignmentStatuses: Record<AssignmentStatus, string> = {
	Planned: "Planned",
	Active: "Active",
	Completed: "Completed",
	Interrupted: "Interrupted",
	DidNotStart: "Did not start",
};

export const assignmentStatusClass: Record<AssignmentStatus, string> = {
	Planned: "badge-draft",
	Active: "badge-active",
	Completed: "badge-completed",
	Interrupted: "badge-cancelled",
	DidNotStart: "badge-inactive",
};

export const assignmentStatusDescriptions: Record<AssignmentStatus, string> = {
	Planned: "Agreed and scheduled. Nobody has started yet.",
	Active: "Running. The person is at work on this posting.",
	Completed: "Ran its course and ended.",
	Interrupted: "Broke off early. Somebody was there and then was not.",
	DidNotStart: "Never began. Deliberately not the same fact as breaking off early.",
};

export const assignmentDocumentCategories: Record<AssignmentDocumentCategory, string> = {
	Contract: "Contract",
	SocialSecurity: "Social security",
	HostCountryNotification: "Host country notification",
	Compliance: "Compliance",
	Other: "Other",
};

/**
 * Mirrors `AssignmentStatusChangePolicy`. A final state is final: there is no restarting a posting,
 * because a repeat engagement is a new one — which is the whole reason assignments exist separately
 * from the person.
 */
export const allowedAssignmentStatusTransitions: Record<AssignmentStatus, AssignmentStatus[]> = {
	Planned: ["Active", "DidNotStart"],
	Active: ["Completed", "Interrupted"],
	Completed: [],
	Interrupted: [],
	DidNotStart: [],
};

/** The two ways out of Active, both of which record when the posting actually ended. */
export const assignmentEndStatuses: AssignmentStatus[] = ["Completed", "Interrupted"];
