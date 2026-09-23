import {
	BriefcaseBusiness,
	CalendarCheck,
	CalendarClock,
	CalendarX,
	FilePlus,
	type LucideIcon,
	MessageSquare,
	PencilLine,
	Repeat,
	Tag,
	UserCheck,
	UserPlus,
	UserX,
} from "lucide-react";
import type {
	InterviewFormat,
	InterviewStatus,
	JobApplicationStatus,
	TimelineEntryType,
	TimelineItem,
} from "@/api/models";
import { formatDateTime } from "@/utlis";
import { applicationStatuses } from "../applications/types";
import { interviewFormats, interviewStatuses, interviewTypes } from "../interviews/type";

export const timelineEntryLabels: Record<TimelineEntryType, string> = {
	CandidateCreated: "Candidate created",
	CandidateUpdated: "Profile updated",
	CandidateTagged: "Tag added",
	CandidateTagRemoved: "Tag removed",
	RegisteredAsWorker: "Registered as worker",
	ApplicationCreated: "Application created",
	ApplicationStatusChanged: "Status changed",
	ApplicationUpdated: "Application updated",
	ApplicationTagged: "Tag added",
	ApplicationTagRemoved: "Tag removed",
	NoteAdded: "Note added",
	InterviewScheduled: "Interview scheduled",
	InterviewRescheduled: "Interview rescheduled",
	InterviewStatusChanged: "Interview status changed",
	InterviewCanceled: "Interview canceled",
	InterviewCompleted: "Interview completed",
	InterviewNoShow: "Interview no-show",
	InterviewerChanged: "Interviewer changed",
	InterviewFormatChanged: "Interview format changed",
};

export const timelineEntryIcons: Record<TimelineEntryType, LucideIcon> = {
	CandidateCreated: UserPlus,
	CandidateUpdated: PencilLine,
	CandidateTagged: Tag,
	CandidateTagRemoved: Tag,
	RegisteredAsWorker: BriefcaseBusiness,
	ApplicationCreated: FilePlus,
	ApplicationStatusChanged: Repeat,
	ApplicationUpdated: PencilLine,
	ApplicationTagged: Tag,
	ApplicationTagRemoved: Tag,
	NoteAdded: MessageSquare,
	InterviewScheduled: CalendarClock,
	InterviewRescheduled: CalendarClock,
	InterviewStatusChanged: CalendarClock,
	InterviewCanceled: CalendarX,
	InterviewCompleted: CalendarCheck,
	InterviewNoShow: UserX,
	InterviewerChanged: UserCheck,
	InterviewFormatChanged: CalendarClock,
};

const transition = (from: string | undefined, to: string | undefined) =>
	from && to ? `${from} → ${to}` : null;

/**
 * The one line under the title: what changed. The API never sends a note's text, so a note entry
 * has nothing to say here.
 */
export function describeTimelineEntry(item: TimelineItem): string | null {
	const interview = item.interviewType ? `${interviewTypes[item.interviewType]} interview` : null;
	const at = item.scheduledAt ? formatDateTime(item.scheduledAt) : null;

	switch (item.type) {
		case "ApplicationStatusChanged":
			return transition(
				applicationStatuses[item.from as JobApplicationStatus],
				applicationStatuses[item.to as JobApplicationStatus],
			);
		case "InterviewScheduled":
		case "InterviewRescheduled":
			return [interview, at].filter(Boolean).join(" · ") || null;
		case "InterviewStatusChanged":
			return transition(
				interviewStatuses[item.from as InterviewStatus],
				interviewStatuses[item.to as InterviewStatus],
			);
		case "InterviewCanceled":
		case "InterviewCompleted":
		case "InterviewNoShow":
			return interview;
		case "InterviewerChanged":
			return transition(item.from ?? undefined, item.to ?? undefined);
		case "InterviewFormatChanged":
			return transition(
				interviewFormats[item.from as InterviewFormat],
				interviewFormats[item.to as InterviewFormat],
			);
		case "CandidateTagged":
		case "CandidateTagRemoved":
		case "ApplicationTagged":
		case "ApplicationTagRemoved":
			return item.to;
		default:
			return null;
	}
}
