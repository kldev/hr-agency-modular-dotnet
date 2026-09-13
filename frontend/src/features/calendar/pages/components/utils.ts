import { isSameDay } from "date-fns";
import type { InterviewProjection } from "@/api/models";

export function getInterviewsForDay(interviews: InterviewProjection[], date: Date) {
	return interviews
		.filter((interview) => isSameDay(new Date(interview.scheduleAt), date))
		.toSorted((a, b) => new Date(a.scheduleAt).getTime() - new Date(b.scheduleAt).getTime());
}
