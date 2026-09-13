import { format } from "date-fns";
import { MapPin, Video } from "lucide-react";
import type { InterviewProjection } from "@/api/models";
import { InterviewStatusBadge } from "@/components/ui";

interface Props {
	interview: InterviewProjection;
	compact?: boolean;

	onClick: (interview: InterviewProjection) => void;
}

export function InterviewCalendarEvent({ interview, compact = false, onClick }: Props) {
	const startsAt = new Date(interview.scheduleAt);

	function handleClick(event: React.MouseEvent) {
		event.stopPropagation();
		onClick(interview);
	}

	if (compact) {
		return (
			<button
				type="button"
				className="interview-calendar-event interview-calendar-event-compact"
				onClick={handleClick}
			>
				<span className="interview-calendar-event-time">{format(startsAt, "HH:mm")}</span>

				<span className="interview-calendar-event-name">{interview.applicantInfo.fullName}</span>
			</button>
		);
	}

	return (
		<button type="button" className="interview-calendar-event" onClick={handleClick}>
			<div className="interview-calendar-event-header">
				<span className="interview-calendar-event-time">{format(startsAt, "HH:mm")}</span>

				<InterviewStatusBadge status={interview.status} />
			</div>

			<div className="interview-calendar-event-name">{interview.applicantInfo.fullName}</div>

			<div className="interview-calendar-event-job">{interview.jobPostTitle}</div>

			<div className="interview-calendar-event-meta">
				{interview.location && (
					<span>
						<MapPin size={12} />
						{interview.location}
					</span>
				)}

				{interview.meetingUrl && (
					<span>
						<Video size={12} />
						Video
					</span>
				)}
			</div>
		</button>
	);
}
