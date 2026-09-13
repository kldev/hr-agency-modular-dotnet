import {
	addMonths,
	addWeeks,
	endOfMonth,
	endOfWeek,
	format,
	startOfMonth,
	startOfWeek,
	subMonths,
	subWeeks,
} from "date-fns";
import { useMemo } from "react";
import type { InterviewProjection } from "@/api/models";
import { InterviewsCalendarToolbar } from "./InterviewsCalendarToolbar";
import { InterviewsMonthView } from "./InterviewsMonthView";
import { InterviewsQuarterView } from "./InterviewsQuarterView";
import { InterviewsWeekView } from "./InterviewsWeekView";
import type { CalendarRange } from "./type";

interface Props {
	range: CalendarRange;
	date: Date;
	interviews: InterviewProjection[];

	onRangeChange: (range: CalendarRange) => void;

	onDateChange: (date: Date) => void;

	onSelectInterview?: (interview: InterviewProjection) => void;

	onSelectDate?: (date: Date) => void;
}

export function InterviewsCalendar({
	range,
	date,
	interviews,
	onRangeChange,
	onDateChange,
	onSelectInterview,
	onSelectDate,
}: Props) {
	const calendarRange = useMemo(() => {
		switch (range) {
			case "week":
				return {
					from: startOfWeek(date, {
						weekStartsOn: 1,
					}),
					to: endOfWeek(date, {
						weekStartsOn: 1,
					}),
				};

			case "month":
				return {
					from: startOfMonth(date),
					to: endOfMonth(date),
				};

			case "quarter":
				return {
					from: startOfMonth(date),
					to: endOfMonth(addMonths(date, 2)),
				};
		}
	}, [date, range]);

	const title = useMemo(() => {
		switch (range) {
			case "week":
				return `${format(calendarRange.from, "d MMM")} – ${format(calendarRange.to, "d MMM yyyy")}`;

			case "month":
				return format(date, "MMMM yyyy");

			case "quarter":
				return `${format(date, "MMM")} – ${format(addMonths(date, 2), "MMM yyyy")}`;
		}
	}, [date, range, calendarRange]);

	function goPrevious() {
		switch (range) {
			case "week":
				onDateChange(subWeeks(date, 1));
				break;

			case "month":
				onDateChange(subMonths(date, 1));
				break;

			case "quarter":
				onDateChange(subMonths(date, 3));
				break;
		}
	}

	function goNext() {
		switch (range) {
			case "week":
				onDateChange(addWeeks(date, 1));
				break;

			case "month":
				onDateChange(addMonths(date, 1));
				break;

			case "quarter":
				onDateChange(addMonths(date, 3));
				break;
		}
	}

	return (
		<section className="interviews-calendar">
			<InterviewsCalendarToolbar
				title={title}
				range={range}
				onRangeChange={onRangeChange}
				onToday={() => onDateChange(new Date())}
				onPrevious={goPrevious}
				onNext={goNext}
			/>

			{range === "week" && (
				<InterviewsWeekView
					date={date}
					interviews={interviews}
					onSelectInterview={onSelectInterview}
					onSelectDate={onSelectDate}
				/>
			)}

			{range === "month" && (
				<InterviewsMonthView
					date={date}
					interviews={interviews}
					onSelectInterview={onSelectInterview}
					onSelectDate={onSelectDate}
				/>
			)}

			{range === "quarter" && (
				<InterviewsQuarterView
					date={date}
					interviews={interviews}
					onSelectInterview={onSelectInterview}
					onSelectDate={onSelectDate}
				/>
			)}
		</section>
	);
}
