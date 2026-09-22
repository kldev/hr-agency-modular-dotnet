import clsx from "clsx";
import {
	eachDayOfInterval,
	endOfMonth,
	endOfWeek,
	isSameMonth,
	isToday,
	startOfMonth,
	startOfWeek,
} from "date-fns";
import { MessageSquare, Moon } from "lucide-react";
import type { WorkDay } from "@/api/models";
import { formatMinutes, type MonthInView, toDateKey, toTimeOfDay } from "../../types";

interface Props {
	month: MonthInView;
	days: WorkDay[];
	/** A month that is not open for editing still shows its days - it just does not take a click. */
	readOnly: boolean;
	onSelectDay: (date: string, day: WorkDay | null) => void;
}

const weekdays = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];

export function TimeSheetCalendar({ month, days, readOnly, onSelectDay }: Props) {
	const anchor = new Date(month.year, month.month - 1, 1);

	const grid = eachDayOfInterval({
		start: startOfWeek(startOfMonth(anchor), { weekStartsOn: 1 }),
		end: endOfWeek(endOfMonth(anchor), { weekStartsOn: 1 }),
	});

	const byDate = new Map(days.map((day) => [day.date.slice(0, 10), day]));

	return (
		<div className="time-sheet-month">
			<div className="time-sheet-month-weekdays">
				{weekdays.map((day) => (
					<div key={day} className="time-sheet-month-weekday">
						{day}
					</div>
				))}
			</div>

			<div className="time-sheet-month-grid">
				{grid.map((date) => {
					const key = toDateKey(date);
					const day = byDate.get(key) ?? null;

					const inMonth = isSameMonth(date, anchor);
					const weekend = date.getDay() === 0 || date.getDay() === 6;

					return (
						<button
							type="button"
							key={key}
							/*
							 * Days outside the month belong to a different sheet - the backend refuses
							 * them by date, so they are not offered rather than refused after a click.
							 */
							disabled={readOnly || !inMonth}
							className={clsx("time-sheet-day", {
								"time-sheet-day-outside": !inMonth,
								"time-sheet-day-weekend": weekend && inMonth,
								"time-sheet-day-filled": Boolean(day),
								"time-sheet-day-readonly": readOnly,
							})}
							onClick={() => onSelectDay(key, day)}
						>
							<span className="time-sheet-day-header">
								<span className={clsx("time-sheet-day-number", isToday(date) && "is-today")}>
									{date.getDate()}
								</span>

								{day?.note ? (
									<MessageSquare
										size={13}
										aria-label="Has a note"
										className="time-sheet-day-icon"
									/>
								) : null}
							</span>

							{day ? (
								<span className="time-sheet-day-body">
									<span className="time-sheet-day-hours">{formatMinutes(Number(day.minutes))}</span>

									<span className="time-sheet-day-from">
										from {toTimeOfDay(day.startsAt)}
										{day.crossesMidnight ? (
											<Moon size={11} aria-label="Runs past midnight" />
										) : null}
									</span>
								</span>
							) : null}
						</button>
					);
				})}
			</div>
		</div>
	);
}
