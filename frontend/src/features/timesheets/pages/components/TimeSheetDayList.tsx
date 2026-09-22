import clsx from "clsx";
import { eachDayOfInterval, endOfMonth, isToday, startOfMonth } from "date-fns";
import { ChevronRight, MessageSquare, Moon } from "lucide-react";
import type { WorkDay } from "@/api/models";
import { formatMinutes, type MonthInView, toDateKey, toTimeOfDay } from "../../types";

interface Props {
	month: MonthInView;
	days: WorkDay[];
	readOnly: boolean;
	onSelectDay: (date: string, day: WorkDay | null) => void;
}

/** Monday first, matching the calendar. `getDay()` counts from Sunday, hence the shift. */
const weekdays = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];

function weekdayOf(date: Date) {
	return weekdays[(date.getDay() + 6) % 7];
}

/**
 * The month as a list, for screens too narrow to hold a calendar. Seven columns on a phone leave
 * about forty pixels per day, which is neither readable nor reliably tappable - so below the
 * breakpoint this replaces the grid rather than trying to squeeze it.
 *
 * Every day of the month is listed, including the empty ones, because the question somebody asks
 * before submitting is which days are still missing - a list of only the filled ones cannot answer
 * it. The leading and trailing days the calendar draws are absent: they pad a grid, and a list has
 * nothing to pad.
 */
export function TimeSheetDayList({ month, days, readOnly, onSelectDay }: Props) {
	const anchor = new Date(month.year, month.month - 1, 1);

	const dates = eachDayOfInterval({
		start: startOfMonth(anchor),
		end: endOfMonth(anchor),
	});

	const byDate = new Map(days.map((day) => [day.date.slice(0, 10), day]));

	return (
		<ul className="time-sheet-day-list data-mobile-view">
			{dates.map((date) => {
				const key = toDateKey(date);
				const day = byDate.get(key) ?? null;
				const weekend = date.getDay() === 0 || date.getDay() === 6;

				return (
					<li key={key}>
						<button
							type="button"
							disabled={readOnly}
							className={clsx("time-sheet-day-row", {
								"time-sheet-day-row-weekend": weekend,
								"time-sheet-day-row-filled": Boolean(day),
								"time-sheet-day-row-readonly": readOnly,
							})}
							onClick={() => onSelectDay(key, day)}
						>
							<span className="time-sheet-day-row-date">
								<span className={clsx("time-sheet-day-number", isToday(date) && "is-today")}>
									{date.getDate()}
								</span>

								<span className="time-sheet-day-row-weekday">{weekdayOf(date)}</span>
							</span>

							<span className="time-sheet-day-row-body">
								{day ? (
									<>
										<span className="time-sheet-day-hours">
											{formatMinutes(Number(day.minutes))}
										</span>

										<span className="time-sheet-day-from">
											from {toTimeOfDay(day.startsAt)}
											{day.crossesMidnight ? (
												<Moon size={11} aria-label="Runs past midnight" />
											) : null}
											{day.note ? <MessageSquare size={11} aria-label="Has a note" /> : null}
										</span>
									</>
								) : (
									<span className="time-sheet-day-row-empty">Not filled in</span>
								)}
							</span>

							{readOnly ? null : <ChevronRight size={16} className="time-sheet-day-row-chevron" />}
						</button>
					</li>
				);
			})}
		</ul>
	);
}
