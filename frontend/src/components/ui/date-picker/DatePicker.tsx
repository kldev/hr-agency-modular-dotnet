import { DateField, DatePicker as HeroDatePicker, Label } from "@heroui/react";
import type { CalendarDate, DateValue } from "@internationalized/date";
import clsx from "clsx";
import type { Locale } from "date-fns";
import { pl } from "date-fns/locale";
import { CalendarDays, ChevronDown, X } from "lucide-react";
import type React from "react";
import { useEffect, useMemo, useRef, useState } from "react";
import { I18nProvider } from "react-aria-components";
import { DatePickerCalendar } from "./DatePickerCalendar";
import {
	boundsError,
	clampDate,
	dayKey,
	fromCalendarDate,
	isDateSeparator,
	isYearIncomplete,
	NOT_A_DATE_MESSAGE,
	parseTypedDate,
	segmentsFill,
	toCalendarDate,
	yearRangeFor,
} from "./datePickerUtils";

export interface DatePickerProps {
	value?: Date | null;
	onChange?: (value: Date | null) => void;

	/** Names the field when it has neither a `label` nor an `aria-label`. */
	placeholder?: string;

	minDate?: Date;
	maxDate?: Date;

	disabled?: boolean;

	clearable?: boolean;

	error?: string;

	/**
	 * Kept for the callers that pass it. The field always shows the panel's `dd.MM.yyyy` - the
	 * segments come from `locale`, and Polish writes a day exactly that way.
	 */
	format?: string;

	locale?: Locale;

	className?: string;

	id?: string;

	name?: string;

	/** A visible label inside the field, which names it for assistive technology too. */
	label?: React.ReactNode;

	"aria-label"?: string;

	/**
	 * A month and a year select in the calendar header. Off by default: most dates here are a few
	 * weeks either side of today - an interview, a start date - and selects would only be noise.
	 * Turn it on where the year is the hard part: a date of birth, a document issued years ago.
	 */
	yearSelect?: boolean;

	/**
	 * The years the select offers. Defaults to `minDate`..`maxDate`, and where either is missing,
	 * 100 years back and 20 ahead of today.
	 */
	yearRange?: { from: number; to: number };
}

const SEGMENT = '[data-slot="date-input-group-segment"]:not([data-type="literal"])';

/*
 * What an empty segment shows. The segments follow the calendar's locale, and Polish would show
 * "dd.mm.rrrr"; the panel is English and has always asked for "dd.mm.yyyy".
 */
const SEGMENT_PLACEHOLDERS: Partial<Record<string, string>> = {
	day: "dd",
	month: "mm",
	year: "yyyy",
};

/** The editable segments of a field, in order. */
function segmentsOf(group: HTMLElement | null): HTMLElement[] {
	return group ? Array.from(group.querySelectorAll<HTMLElement>(SEGMENT)) : [];
}

/**
 * HeroUI's `DatePicker`: a segmented field (`DateField`) and a calendar in a React Aria popover.
 * The API is the panel's own and speaks `Date`; the conversion to `CalendarDate` stays in here.
 *
 * What the panel adds on top of the segments, because a date is typed far more often than picked:
 *
 * - "1.03.1959", "1-3-1959" - a separator moves to the next part even before it is full, so a
 *   one digit day does not swallow the month's first digit. "07031959" needs nothing: segments
 *   move on when full.
 * - Pasting "07.03.1959", "07031959", "07-03-1959" or an ISO date fills the whole field - a
 *   segment on its own takes digits one at a time and would ignore a pasted string.
 * - A day outside `minDate`/`maxDate`, and a half-typed one on leaving the field, stay in the
 *   field with the reason under it; the form keeps its last good value until they are fixed.
 */
export function DatePicker({
	value = null,
	onChange,
	placeholder = "Select date",
	minDate,
	maxDate,
	disabled = false,
	clearable = true,
	error,
	locale = pl,
	className,
	id,
	name,
	label,
	"aria-label": ariaLabel,
	yearSelect = false,
	yearRange,
}: DatePickerProps) {
	const groupRef = useRef<HTMLDivElement>(null);

	/*
	 * What the field shows. It follows `value` whenever the value changes from outside - a pick,
	 * a form reset - but is also allowed to hold a day the form does not take (outside the limits),
	 * so a refused entry stays on screen. React Aria resets its segments whenever this object
	 * changes, so it is only ever replaced when the day really changes.
	 */
	const [fieldValue, setFieldValue] = useState<CalendarDate | null>(() => toCalendarDate(value));

	/*
	 * Why the entry was not taken. The entry stays in the field next to it - wiping what somebody
	 * typed because of one wrong digit makes them type it all again.
	 */
	const [typedError, setTypedError] = useState<string | null>(null);

	const [open, setOpen] = useState(false);

	const [focusedValue, setFocusedValue] = useState<CalendarDate>(
		() => toCalendarDate(clampDate(value ?? new Date(), minDate, maxDate)) as CalendarDate,
	);

	const valueKey = dayKey(value);

	// biome-ignore lint/correctness/useExhaustiveDependencies: follows the day, not the Date object
	useEffect(() => {
		setFieldValue((current) =>
			dayKey(fromCalendarDate(current)) === valueKey ? current : toCalendarDate(value),
		);
		setTypedError(null);
	}, [valueKey]);

	const years = useMemo(
		() =>
			yearSelect
				? yearRangeFor({ yearRange, minDate, maxDate, currentYear: new Date().getFullYear() })
				: undefined,
		[yearSelect, yearRange, minDate, maxDate],
	);

	/** Takes a whole day - typed to the end, picked, pasted - or refuses it with a reason. */
	const accept = (next: DateValue | null) => {
		const date = fromCalendarDate(next);

		if (!date) {
			setTypedError(null);

			if (clearable) {
				setFieldValue(null);
				if (value) onChange?.(null);
			} else {
				// Nothing to fall back on but the last day the form holds.
				setFieldValue(toCalendarDate(value));
			}

			return;
		}

		setFieldValue(toCalendarDate(date));

		if (isYearIncomplete(next)) {
			// Shown as typed, judged on the last digit or on leaving the field.
			setTypedError(null);
			return;
		}

		const outside = boundsError(date, minDate, maxDate);

		setTypedError(outside);

		if (!outside && dayKey(date) !== valueKey) {
			onChange?.(date);
		}
	};

	/** Leaving the field with only part of a date in it is a mistake worth saying out loud. */
	const handleBlur = () => {
		const fill = segmentsFill(
			segmentsOf(groupRef.current).map((segment) => !segment.hasAttribute("data-placeholder")),
		);

		if (fill === "partial" || isYearIncomplete(fieldValue)) {
			setTypedError(NOT_A_DATE_MESSAGE);
		} else if (fill === "empty" && typedError === NOT_A_DATE_MESSAGE) {
			setTypedError(null);
		}
	};

	const handlePaste = (event: React.ClipboardEvent) => {
		const text = event.clipboardData.getData("text");

		if (!text) {
			return;
		}

		event.preventDefault();

		const parsed = parseTypedDate(text);

		if (parsed === undefined) {
			setTypedError(NOT_A_DATE_MESSAGE);
			return;
		}

		accept(toCalendarDate(parsed));
	};

	/*
	 * Whether the focused segment got a digit since it took focus. A separator only moves on from
	 * a segment that has one: after "07" the field has already moved to the month by itself, and
	 * the dot that follows must not skip the month too.
	 */
	const typedInSegment = useRef(false);

	const handleKeyDown = (event: React.KeyboardEvent) => {
		const target = event.target as HTMLElement;

		if (!target.matches(SEGMENT)) {
			return;
		}

		if (/^\d$/.test(event.key)) {
			typedInSegment.current = true;
			return;
		}

		if (!isDateSeparator(event.key)) {
			return;
		}

		event.preventDefault();

		if (!typedInSegment.current) {
			return;
		}

		const segments = segmentsOf(groupRef.current);
		segments[segments.indexOf(target) + 1]?.focus();
	};

	const handleOpenChange = (isOpen: boolean) => {
		if (isOpen) {
			setFocusedValue(
				fieldValue ?? (toCalendarDate(clampDate(new Date(), minDate, maxDate)) as CalendarDate),
			);
		}

		setOpen(isOpen);
	};

	const handleClear = () => {
		setTypedError(null);
		setFieldValue(null);
		setOpen(false);
		if (value) onChange?.(null);
		segmentsOf(groupRef.current)[0]?.focus();
	};

	const handleToday = () => {
		const today = clampDate(new Date(), minDate, maxDate);

		accept(toCalendarDate(today));
		setOpen(false);
	};

	const message = typedError ?? error;

	return (
		// The segments, the placeholders and the calendar speak the panel's locale, set here
		// rather than for the whole app: the rest of the panel is English, and React Aria would
		// otherwise take the browser's locale and render differently on the server.
		<I18nProvider locale={locale.code ?? "pl"}>
			<HeroDatePicker
				className={clsx("panel-date-picker", className)}
				id={id}
				name={name}
				aria-label={label ? undefined : (ariaLabel ?? placeholder)}
				value={fieldValue}
				onChange={accept}
				onBlur={handleBlur}
				// No minValue/maxValue on the field: React Aria would mark it invalid the moment the
				// first digit of a year is typed. The limits are checked once the day is whole, and
				// the calendar gets them below to grey out the days beyond.
				isDisabled={disabled}
				isInvalid={Boolean(message)}
				validationBehavior="aria"
				granularity="day"
				shouldForceLeadingZeros
				isOpen={open}
				onOpenChange={handleOpenChange}
			>
				{label && <Label className="form-label">{label}</Label>}

				<DateField.Group
					ref={groupRef}
					fullWidth
					className="panel-date-field"
					onKeyDown={handleKeyDown}
					onFocus={() => {
						typedInSegment.current = false;
					}}
					onPaste={handlePaste}
				>
					<DateField.Prefix>
						<HeroDatePicker.Trigger
							aria-label="Open calendar"
							className="panel-date-field__trigger"
						>
							<CalendarDays size={16} strokeWidth={1.8} />
						</HeroDatePicker.Trigger>
					</DateField.Prefix>

					<DateField.Input>
						{(segment) => (
							<DateField.Segment segment={segment}>
								{({ isPlaceholder, text, type }) =>
									isPlaceholder ? (SEGMENT_PLACEHOLDERS[type] ?? text) : text
								}
							</DateField.Segment>
						)}
					</DateField.Input>

					<DateField.Suffix>
						{/* Decorative, like the arrow of a select; the calendar icon is the control. */}
						<ChevronDown
							size={16}
							aria-hidden
							className={clsx("panel-date-field__chevron", open && "rotate-180")}
							onClick={() => !disabled && handleOpenChange(!open)}
						/>

						{fieldValue && clearable && !disabled && (
							// A plain button: a React Aria one would take the trigger's props from the picker.
							<button
								type="button"
								aria-label="Clear date"
								className="panel-date-field__clear"
								onClick={handleClear}
							>
								<X size={14} />
							</button>
						)}
					</DateField.Suffix>
				</DateField.Group>

				{message && <div className="panel-date-field__error">{message}</div>}

				<HeroDatePicker.Popover placement="bottom start" className="panel-date-popover">
					<DatePickerCalendar
						focusedValue={focusedValue}
						onFocusChange={setFocusedValue}
						minDate={minDate}
						maxDate={maxDate}
						locale={locale}
						onToday={handleToday}
						years={years}
					/>
				</HeroDatePicker.Popover>
			</HeroDatePicker>
		</I18nProvider>
	);
}

export default DatePicker;
