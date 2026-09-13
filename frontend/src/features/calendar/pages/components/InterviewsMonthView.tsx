import {
	eachDayOfInterval,
	endOfMonth,
	endOfWeek,
	format,
	isSameDay,
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

export function InterviewsMonthView({ date, interviews, onSelectInterview, onSelectDate }: Props) {
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
		<div className="interviews-calendar-scroll">
			<div className="interviews-month">
				<div className="interviews-month-weekdays">
					{["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"].map(
						(day) => (
							<div key={day} className="interviews-month-weekday">
								{day}
							</div>
						),
					)}
				</div>

				<div className="interviews-month-grid">
					{days.map((day) => {
						const dayInterviews = getInterviewsForDay(interviews, day);

						const currentMonth = isSameMonth(day, date);

						return (
							/// div
							<button
								type="button"
								key={day.toISOString()}
								className={["interviews-month-day", !currentMonth && "interviews-month-day-outside"]
									.filter(Boolean)
									.join(" ")}
								onClick={() => onSelectDate?.(day)}
							>
								<div className="interviews-month-day-header">
									<span
										className={[
											"interviews-day-number",
											isToday(day) && "interviews-day-number-today",
										]
											.filter(Boolean)
											.join(" ")}
									>
										{format(day, "d")}
									</span>

									{dayInterviews.length > 0 && (
										<span className="interviews-day-count">{dayInterviews.length}</span>
									)}
								</div>

								<div className="interviews-month-events">
									{dayInterviews.slice(0, 4).map((interview) => (
										<InterviewCalendarEvent
											key={interview.id}
											interview={interview}
											onClick={onSelectInterview ?? (() => { })}
										/>
									))}

									{dayInterviews.length > 4 && (
										<button
											type="button"
											className="interviews-more-button"
											onClick={(event) => {
												event.stopPropagation();
												onSelectDate?.(day);
											}}
										>
											+{dayInterviews.length - 4} more
										</button>
									)}
								</div>
							</button>
						);
					})}
				</div>
			</div>
		</div>
	);
}


