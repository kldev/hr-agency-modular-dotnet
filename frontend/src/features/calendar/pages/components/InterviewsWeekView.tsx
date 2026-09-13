import { eachDayOfInterval, endOfWeek, format, isToday, startOfWeek } from "date-fns";
import type { InterviewProjection } from "@/api/models";
import { InterviewCalendarEvent } from "./InterviewCalendarEvent";
import { getInterviewsForDay } from "./utils";

interface Props {
	date: Date;
	interviews: InterviewProjection[];

	onSelectInterview?: (interview: InterviewProjection) => void;

	onSelectDate?: (date: Date) => void;
}

const START_HOUR = 8;
const END_HOUR = 19;
const HOUR_HEIGHT = 72;
const TIME_COLUMN_WIDTH = 72;

export function InterviewsWeekView({ date, interviews, onSelectInterview, onSelectDate }: Props) {
	const weekStart = startOfWeek(date, {
		weekStartsOn: 1,
	});

	const weekEnd = endOfWeek(date, {
		weekStartsOn: 1,
	});

	const days = eachDayOfInterval({
		start: weekStart,
		end: weekEnd,
	});

	const hours = Array.from(
		{
			length: END_HOUR - START_HOUR,
		},
		(_, index) => START_HOUR + index,
	);

	return (
		<div className="interviews-calendar-scroll">
			<div className="interviews-week">
				{/* Header */}
				<div
					className="interviews-week-header"
					style={{
						gridTemplateColumns: `72px repeat(7, minmax(160px, 1fr))`,
					}}
				>
					<div />

					{days.map((day) => (
						/// div
						<button
							type="button"
							key={day.toISOString()}
							className={[
								"interviews-week-day-header",
								isToday(day) && "interviews-week-day-header-today",
							]
								.filter(Boolean)
								.join(" ")}
							onClick={() => onSelectDate?.(day)}
						>
							<span>{format(day, "EEE")}</span>

							<strong>{format(day, "d")}</strong>
						</button>
					))}
				</div>

				{/* Calendar body */}
				<div
					className="interviews-week-body"
					style={{
						position: "relative",
						height: hours.length * HOUR_HEIGHT,
						minWidth: `${TIME_COLUMN_WIDTH + 7 * 160}px`,
					}}
				>
					{/* Grid with hours */}
					<div
						className="interviews-week-grid"
						style={{
							display: "grid",
							gridTemplateColumns: `${TIME_COLUMN_WIDTH}px repeat(7, minmax(160px, 1fr))`,
							gridTemplateRows: `repeat(${hours.length}, ${HOUR_HEIGHT}px)`,
							height: "100%",
						}}
					>
						{hours.map((hour) => (
							<div
								key={hour}
								className="interviews-week-row"
								style={{
									gridColumn: "1 / -1",
									display: "grid",
									gridTemplateColumns: `${TIME_COLUMN_WIDTH}px repeat(7, minmax(160px, 1fr))`,
								}}
							>
								<div className="interviews-week-time">{formatHour(hour)}</div>

								{days.map((day) => (
									// biome-ignore lint/a11y/noStaticElementInteractions: false
									// biome-ignore lint/a11y/useKeyWithClickEvents: false
									<div
										key={`${day.toISOString()}-${hour}`}
										className="interviews-week-cell"
										onClick={() => onSelectDate?.(day)}
									/>
								))}
							</div>
						))}
					</div>

					{/* Events overlay */}
					<div
						className="interviews-week-events"
						style={{
							position: "absolute",
							top: 0,
							right: 0,
							bottom: 0,
							left: TIME_COLUMN_WIDTH,
							display: "grid",
							gridTemplateColumns: "repeat(7, minmax(160px, 1fr))",
							pointerEvents: "none",
						}}
					>
						{days.map((day) => {
							const dayInterviews = getInterviewsForDay(interviews, day);

							return (
								<div
									key={day.toISOString()}
									className="interviews-week-events-column"
									style={{
										position: "relative",
										height: "100%",
									}}
								>
									{dayInterviews.map((interview) => (
										<WeekEvent
											key={interview.id}
											interview={interview}
											onClick={onSelectInterview ?? (() => {})}
										/>
									))}
								</div>
							);
						})}
					</div>
				</div>
			</div>
		</div>
	);
}

function WeekEvent({
	interview,
	onClick,
}: {
	interview: InterviewProjection;
	onClick: (interview: InterviewProjection) => void;
}) {
	const start = new Date(interview.scheduleAt);

	const minutesFromStart = (start.getHours() - START_HOUR) * 60 + start.getMinutes();

	const top = (minutesFromStart / 60) * HOUR_HEIGHT;

	const height = Math.max(46, (30 / 60) * HOUR_HEIGHT);

	return (
		<div
			className="interviews-week-event-position"
			style={{
				position: "absolute",
				top,
				left: 4,
				right: 4,
				height,
				pointerEvents: "auto",
				zIndex: 10,
			}}
		>
			<InterviewCalendarEvent interview={interview} onClick={onClick} />
		</div>
	);
}

function formatHour(hour: number) {
	return `${String(hour).padStart(2, "0")}:00`;
}
