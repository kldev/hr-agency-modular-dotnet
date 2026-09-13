import {
	addMonths,
	eachDayOfInterval,
	endOfMonth,
	endOfWeek,
	format,
	isSameMonth,
	isToday,
	startOfMonth,
	startOfWeek,
} from "date-fns";
import type { InterviewProjection } from "@/api/models";
import { InterviewCalendarEvent } from "./InterviewCalendarEvent";
import { getInterviewsForDay } from "./utils";

interface Props {
	date: Date;
	interviews: InterviewProjection[];

	onSelectInterview?: (interview: InterviewProjection) => void;

	onSelectDate?: (date: Date) => void;
}

export function InterviewsQuarterView({
	date,
	interviews,
	onSelectInterview,
	onSelectDate,
}: Props) {
	const months = [
		startOfMonth(date),
		startOfMonth(addMonths(date, 1)),
		startOfMonth(addMonths(date, 2)),
	];

	return (
		<div className="interviews-calendar-scroll">
			<div className="interviews-quarter">
				{months.map((month) => (
					<QuarterMonth
						key={month.toISOString()}
						date={month}
						interviews={interviews}
						onSelectInterview={onSelectInterview}
						onSelectDate={onSelectDate}
					/>
				))}
			</div>
		</div>
	);
}

function QuarterMonth({ date, interviews, onSelectInterview, onSelectDate }: Props) {
	const firstDay = startOfWeek(startOfMonth(date), {
		weekStartsOn: 1,
	});

	const lastDay = endOfWeek(endOfMonth(date), {
		weekStartsOn: 1,
	});

	const days = eachDayOfInterval({
		start: firstDay,
		end: lastDay,
	});

	return (
		<div className="interviews-quarter-month">
			<header className="interviews-quarter-month-header">{format(date, "MMMM yyyy")}</header>

			<div className="interviews-quarter-weekdays">
				{["M", "T", "W", "T", "F", "S", "S"].map((day, index) => (
					<span key={`${day}-${index}`}>{day}</span>
				))}
			</div>

			<div className="interviews-quarter-grid">
				{days.map((day) => {
					const dayInterviews = getInterviewsForDay(interviews, day);

					const currentMonth = isSameMonth(day, date);

					return (
						// biome-ignore lint/a11y/useKeyWithClickEvents: false
						// biome-ignore lint/a11y/noStaticElementInteractions: false
						<div
							key={day.toISOString()}
							className={[
								"interviews-quarter-day",
								!currentMonth && "interviews-quarter-day-outside",
							]
								.filter(Boolean)
								.join(" ")}
							onClick={() => onSelectDate?.(day)}
						>
							<span
								className={[
									"interviews-quarter-day-number",
									isToday(day) && "interviews-quarter-day-today",
								]
									.filter(Boolean)
									.join(" ")}
							>
								{format(day, "d")}
							</span>

							<div className="interviews-quarter-events">
								{dayInterviews.slice(0, 2).map((interview) => (
									<InterviewCalendarEvent
										key={interview.id}
										interview={interview}
										compact
										onClick={onSelectInterview ?? (() => {})}
									/>
								))}

								{dayInterviews.length > 2 && (
									<button
										type="button"
										className="interviews-quarter-more"
										onClick={(event) => {
											event.stopPropagation();
											onSelectDate?.(day);
										}}
									>
										+{dayInterviews.length - 2}
									</button>
								)}
							</div>
						</div>
					);
				})}
			</div>
		</div>
	);
}
