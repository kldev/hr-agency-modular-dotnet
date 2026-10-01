import { Calendar } from "@heroui/react";
import type { CalendarDate } from "@internationalized/date";
import type { Locale } from "date-fns";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { useMemo } from "react";
import {
	formatCalendarMonth,
	fromCalendarDate,
	monthNames,
	toCalendarDate,
	yearOptions,
} from "./datePickerUtils";

export interface DatePickerCalendarProps {
	/** The month in view, as the day the calendar keeps its focus on. */
	focusedValue: CalendarDate;
	onFocusChange: (value: CalendarDate) => void;
	locale: Locale;
	minDate?: Date;
	maxDate?: Date;
	onToday: () => void;
	/**
	 * When given, the header becomes a month select and a year select over these. Without it the
	 * header is text and the only way to another year is twelve clicks - fine for an interview
	 * next week, hopeless for a date of birth.
	 */
	years?: number[];
}

/**
 * The calendar inside `DatePicker`'s popover. The selected day and the selection itself come
 * from the picker through React Aria's context; this draws the month, greys out the days beyond
 * the limits and moves between months.
 *
 * The selects are native on purpose: they sit inside a popover inside a dialog, a third layer of
 * overlay would only add a place for focus to get lost, and a native select is keyboard-friendly
 * and opens over everything by itself.
 */
export function DatePickerCalendar({
	focusedValue,
	onFocusChange,
	locale,
	minDate,
	maxDate,
	onToday,
	years,
}: DatePickerCalendarProps) {
	const months = useMemo(() => monthNames(locale), [locale]);

	// Always passed: HeroUI falls back to 1900..2099 of its own, which would beat the picker's.
	const minValue = useMemo(() => toCalendarDate(minDate) ?? undefined, [minDate]);
	const maxValue = useMemo(() => toCalendarDate(maxDate) ?? undefined, [maxDate]);

	return (
		<div className="date-picker-calendar">
			<Calendar
				aria-label="Calendar"
				className="date-picker-calendar__calendar"
				focusedValue={focusedValue}
				onFocusChange={(value) => onFocusChange(value as CalendarDate)}
				minValue={minValue}
				maxValue={maxValue}
				firstDayOfWeek="mon"
			>
				<Calendar.Header className="date-picker-calendar__header">
					<Calendar.NavButton slot="previous" aria-label="Previous month">
						<ChevronLeft size={17} />
					</Calendar.NavButton>

					{years ? (
						<div className="date-picker-calendar__selects">
							<select
								aria-label="Month"
								className="date-picker-calendar__select"
								value={focusedValue.month}
								onChange={(event) =>
									onFocusChange(focusedValue.set({ month: Number(event.target.value) }))
								}
							>
								{months.map((name, index) => (
									<option key={name} value={index + 1}>
										{name}
									</option>
								))}
							</select>

							<select
								aria-label="Year"
								className="date-picker-calendar__select"
								value={focusedValue.year}
								onChange={(event) =>
									onFocusChange(focusedValue.set({ year: Number(event.target.value) }))
								}
							>
								{/* The month in view may sit outside the range; it still has to be selectable. */}
								{yearOptions(years, focusedValue.year).map((year) => (
									<option key={year} value={year}>
										{year}
									</option>
								))}
							</select>
						</div>
					) : (
						<div className="date-picker-calendar__heading">
							{formatCalendarMonth(fromCalendarDate(focusedValue) ?? new Date(), locale)}
						</div>
					)}

					<Calendar.NavButton slot="next" aria-label="Next month">
						<ChevronRight size={17} />
					</Calendar.NavButton>
				</Calendar.Header>

				<Calendar.Grid weekdayStyle="narrow">
					<Calendar.GridHeader>
						{(day) => <Calendar.HeaderCell>{day}</Calendar.HeaderCell>}
					</Calendar.GridHeader>
					<Calendar.GridBody>{(date) => <Calendar.Cell date={date} />}</Calendar.GridBody>
				</Calendar.Grid>
			</Calendar>

			<div className="date-picker-calendar__footer">
				{/*
				 * A plain button: a React Aria one here would take the picker's own trigger props
				 * from context and open the calendar instead of picking today.
				 */}
				<button type="button" className="date-picker-calendar__today" onClick={onToday}>
					Today
				</button>

				<span className="date-picker-calendar__hint">Select a date</span>
			</div>
		</div>
	);
}
