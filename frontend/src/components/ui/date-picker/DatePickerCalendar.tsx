import clsx from "clsx";
import type { Locale } from "date-fns";
import { addMonths, format, subMonths } from "date-fns";
import { ChevronLeft, ChevronRight } from "lucide-react";
import {
	createCalendarDays,
	createWeekDays,
	formatCalendarMonth,
	formatCalendarWeekday,
	isDateDisabled,
	isSelectedDate,
	isToday,
} from "./datePickerUtils";

export interface DatePickerCalendarProps {
	month: Date;
	selectedDate: Date | null;
	minDate?: Date;
	maxDate?: Date;
	locale: Locale;
	onMonthChange: (month: Date) => void;
	onSelect: (date: Date) => void;
	onToday: () => void;
	/**
	 * When given, the year in the header becomes a select over these. Without it the header is
	 * text and the only way to another year is twelve clicks - fine for an interview next week,
	 * hopeless for a date of birth.
	 */
	years?: number[];
}

export function DatePickerCalendar({
	month,
	selectedDate,
	minDate,
	maxDate,
	locale,
	onMonthChange,
	onSelect,
	onToday,
	years,
}: DatePickerCalendarProps) {
	const today = new Date();

	const days = createCalendarDays(month, locale);
	const weekDays = createWeekDays(month, locale);

	const previousMonth = subMonths(month, 1);
	const nextMonth = addMonths(month, 1);

	const canNavigatePrevious =
		!minDate ||
		subMonths(month, 1).getTime() >=
			new Date(minDate.getFullYear(), minDate.getMonth(), 1).getTime();

	const canNavigateNext =
		!maxDate ||
		addMonths(month, 1).getTime() <=
			new Date(maxDate.getFullYear(), maxDate.getMonth(), 1).getTime();

	return (
		<div
			className="
        w-[320px]
        rounded-sm
        border
        border-(--color-border)
        bg-(--color-surface)
        p-3
        shadow-[0_8px_24px_rgba(0,0,0,0.12)]
      "
			role="dialog"
			aria-label="Calendar"
		>
			{/* Header */}
			<div className="mb-3 flex items-center justify-between">
				<button
					type="button"
					onClick={() => onMonthChange(previousMonth)}
					disabled={!canNavigatePrevious}
					aria-label="Previous month"
					className={clsx(
						"inline-flex h-8 w-8 items-center justify-center rounded-[3px]",
						"text-(--color-text-secondary)",
						"transition-colors",
						"hover:bg-(--color-surface-hover)",
						"hover:text-(--color-text)",
						"focus:outline-none focus:ring-2 focus:ring-(--color-primary-soft)",
						"disabled:pointer-events-none disabled:opacity-40",
					)}
				>
					<ChevronLeft size={17} />
				</button>

				{years ? (
					<div className="flex items-center gap-1.5 text-sm font-semibold text-(--color-text)">
						<span className="select-none">{capitalise(format(month, "LLLL", { locale }))}</span>

						<select
							aria-label="Year"
							value={month.getFullYear()}
							onChange={(event) =>
								onMonthChange(new Date(Number(event.target.value), month.getMonth(), 1))
							}
							className={clsx(
								"h-8 rounded-[3px] border border-(--color-border) bg-(--color-surface) px-1.5",
								"text-sm font-semibold text-(--color-text)",
								"hover:border-(--color-border-strong)",
								"focus:outline-none focus:ring-2 focus:ring-(--color-primary-soft)",
							)}
						>
							{/* The month in view may sit outside the range; it still has to be selectable. */}
							{(years.includes(month.getFullYear()) ? years : [...years, month.getFullYear()])
								.sort((a, b) => a - b)
								.map((year) => (
									<option key={year} value={year}>
										{year}
									</option>
								))}
						</select>
					</div>
				) : (
					<div
						className="
            select-none
            text-sm
            font-semibold
            text-(--color-text)
          "
					>
						{formatCalendarMonth(month, locale)}
					</div>
				)}

				<button
					type="button"
					onClick={() => onMonthChange(nextMonth)}
					disabled={!canNavigateNext}
					aria-label="Next month"
					className={clsx(
						"inline-flex h-8 w-8 items-center justify-center rounded-[3px]",
						"text-(--color-text-secondary)",
						"transition-colors",
						"hover:bg-(--color-surface-hover)",
						"hover:text-(--color-text)",
						"focus:outline-none focus:ring-2 focus:ring-(--color-primary-soft)",
						"disabled:pointer-events-none disabled:opacity-40",
					)}
				>
					<ChevronRight size={17} />
				</button>
			</div>

			{/* Week days */}
			<div className="mb-1 grid grid-cols-7">
				{weekDays.map((day) => (
					<div
						key={day.toISOString()}
						className="
              flex
              h-8
              items-center
              justify-center
              text-[11px]
              font-semibold
              uppercase
              tracking-wide
              text-(--color-text-muted)
            "
					>
						{formatCalendarWeekday(day, locale)}
					</div>
				))}
			</div>

			{/* Days */}
			<div className="grid grid-cols-7 gap-y-1">
				{days.map(({ date, currentMonth }) => {
					const disabled = isDateDisabled(date, minDate, maxDate);

					const selected = isSelectedDate(date, selectedDate);

					const todayDate = isToday(date, today);

					return (
						<button
							key={date.toISOString()}
							type="button"
							disabled={disabled}
							onClick={() => onSelect(date)}
							aria-label={date.toLocaleDateString()}
							aria-current={todayDate ? "date" : undefined}
							className={clsx(
								"mx-auto flex h-9 w-9 items-center justify-center rounded-[3px]",
								"text-sm",
								"transition-colors",
								"focus:outline-none focus:ring-2 focus:ring-(--color-primary-soft)",

								currentMonth ? "text-(--color-text)" : "text-(--color-text-muted)",

								!currentMonth && "opacity-50",

								!disabled && !selected && "hover:bg-(--color-surface-hover)",

								disabled && "cursor-not-allowed opacity-30",

								selected && "bg-(--color-primary)! text-white! hover:bg-(--color-primary-hover)!",

								todayDate && !selected && "font-semibold ring-1 ring-inset ring-(--color-primary)",
							)}
						>
							{date.getDate()}
						</button>
					);
				})}
			</div>

			{/* Footer */}
			<div
				className="
          mt-3
          flex
          items-center
          justify-between
          border-t
          border-(--color-border-subtle)
          pt-3
        "
			>
				<button
					type="button"
					onClick={onToday}
					className="
            rounded-[3px]
            px-2
            py-1.5
            text-xs
            font-medium
            text-(--color-primary)
            transition-colors
            hover:bg-(--color-primary-soft)
            focus:outline-none
            focus:ring-2
            focus:ring-(--color-primary-soft)
          "
				>
					Today
				</button>

				<span className="text-[11px] text-(--color-text-muted)">Select a date</span>
			</div>
		</div>
	);
}

function capitalise(value: string): string {
	return value.charAt(0).toUpperCase() + value.slice(1);
}
