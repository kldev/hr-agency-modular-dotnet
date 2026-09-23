import {
	BadgeCheck,
	ClipboardCheck,
	Handshake,
	Inbox,
	type LucideIcon,
	MessagesSquare,
	Undo2,
	UserSearch,
	XCircle,
} from "lucide-react";
import type { JobApplicationStatus } from "#/api/models";

export const applicationStatusIcons: Record<JobApplicationStatus, LucideIcon> = {
	Applied: Inbox,
	Screening: UserSearch,
	Interview: MessagesSquare,
	Assessment: ClipboardCheck,
	Offer: Handshake,
	Hired: BadgeCheck,
	Rejected: XCircle,
	Withdrawn: Undo2,
};

/** The funnel in the order it is walked, which is the order of the columns. */
export const applicationPipeline = Object.keys(applicationStatusIcons) as JobApplicationStatus[];
