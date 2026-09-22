import clsx from "clsx";
import type { Locale } from "date-fns";
import { pl } from "date-fns/locale";
import { CalendarDays, ChevronDown, X } from "lucide-react";
import { useCallback, useEffect, useLayoutEffect, useMemo, useRef, useState } from "react";
import { createPortal } from "react-dom";
import { DatePickerCalendar } from "./DatePickerCalendar";
import {
	clampDate,
	isDateDisabled,
	normalizeDate,
	parseTypedDate,
	withDateSeparators,
	yearsBetween,
} from "./datePickerUtils";

/** How far back and ahead a year select reaches when neither a range nor a limit says otherwise. */
const DEFAULT_YEARS_BACK = 100;
const DEFAULT_YEARS_AHEAD = 20;

/** Enough room to decide whether the calendar still fits below the field before it is measured. */
const ESTIMATED_CALENDAR_HEIGHT = 340;

const VIEWPORT_MARGIN = 8;

const FIELD_GAP = 6;

export interface DatePickerProps {
	value?: Date | null;
	onChange?: (value: Date | null) => void;

	placeholder?: string;

	minDate?: Date;
	maxDate?: Date;

	disabled?: boolean;

	clearable?: boolean;

	error?: string;

	format?: string;

	locale?: Locale;

	className?: string;

	id?: string;

	name?: string;

	"aria-label"?: string;

	/**
	 * A year select in the calendar header. Off by default: most dates here are a few weeks either
	 * side of today - an interview, a start date - and a select would only be noise. Turn it on
	 * where the year is the hard part: a date of birth, a document issued years ago.
	 */
	yearSelect?: boolean;

	/**
	 * The years the select offers. Defaults to `minDate`..`maxDate`, and where either is missing,
	 * 100 years back and 20 ahead of today.
	 */
	yearRange?: { from: number; to: number };
}

export function DatePicker({
	value = null,
	onChange,
	placeholder = "Select date",
	minDate,
	maxDate,
	disabled = false,
	clearable = true,
	error,
	//format: dateFormat = "dd.MM.yyyy",
	locale = pl,
	className,
	id,
	name,
	"aria-label": ariaLabel,
	yearSelect = false,
	yearRange,
}: DatePickerProps) {
	const rootRef = useRef<HTMLDivElement>(null);
	const inputRef = useRef<HTMLInputElement>(null);
	const popoverRef = useRef<HTMLDivElement>(null);

	const [open, setOpen] = useState(false);

	const [position, setPosition] = useState<{ top: number; left: number } | null>(null);

	const [visibleMonth, setVisibleMonth] = useState<Date>(normalizeDate(value ?? new Date()));

	useEffect(() => {
		if (value) {
			setVisibleMonth(normalizeDate(value));
		}
	}, [value]);

	/*
	 * The calendar is rendered into the body rather than next to the field, so it is positioned by
	 * hand. Anything else would put it inside whichever scroll container the field happens to sit in
	 * - and the wizard dialog clips its own overflow, which is where a half-cut calendar came from.
	 */
	const updatePosition = useCallback(() => {
		const anchor = rootRef.current;

		if (!anchor) {
			return;
		}

		const field = anchor.getBoundingClientRect();
		const calendar = popoverRef.current?.getBoundingClientRect();

		const height = calendar?.height ?? ESTIMATED_CALENDAR_HEIGHT;
		const width = calendar?.width ?? field.width;

		const fitsBelow = window.innerHeight - field.bottom >= height + FIELD_GAP + VIEWPORT_MARGIN;
		const fitsAbove = field.top >= height + FIELD_GAP + VIEWPORT_MARGIN;

		// Below by default; above only when there is genuinely no room below but there is above.
		const top = fitsBelow || !fitsAbove ? field.bottom + FIELD_GAP : field.top - height - FIELD_GAP;

		const left = Math.max(
			VIEWPORT_MARGIN,
			Math.min(field.left, window.innerWidth - width - VIEWPORT_MARGIN),
		);

		setPosition({ top, left });
	}, []);

	useLayoutEffect(() => {
		if (!open) {
			setPosition(null);
			return;
		}

		updatePosition();

		// Capture, so scrolling any ancestor - including a dialog body - moves the calendar with it.
		window.addEventListener("scroll", updatePosition, true);
		window.addEventListener("resize", updatePosition);

		return () => {
			window.removeEventListener("scroll", updatePosition, true);
			window.removeEventListener("resize", updatePosition);
		};
	}, [open, updatePosition]);

	useEffect(() => {
		if (!open) {
			return;
		}

		const handlePointerDown = (event: PointerEvent) => {
			const target = event.target as Node;

			const insideField = rootRef.current?.contains(target) ?? false;

			// The calendar is no longer a descendant of the field, so it has to be asked separately.
			const insideCalendar = popoverRef.current?.contains(target) ?? false;

			if (!insideField && !insideCalendar) {
				setOpen(false);
			}
		};

		const handleKeyDown = (event: KeyboardEvent) => {
			if (event.key === "Escape") {
				setOpen(false);

				requestAnimationFrame(() => {
					inputRef.current?.focus();
				});
			}
		};

		document.addEventListener("pointerdown", handlePointerDown);

		document.addEventListener("keydown", handleKeyDown);

		return () => {
			document.removeEventListener("pointerdown", handlePointerDown);

			document.removeEventListener("keydown", handleKeyDown);
		};
	}, [open]);

	const formatted = useCallback(
		(date: Date | null) =>
			date
				? new Intl.DateTimeFormat(locale.code, {
						day: "2-digit",
						month: "2-digit",
						year: "numeric",
					}).format(date)
				: "",
		[locale.code],
	);

	/*
	 * What is in the field while somebody types. It follows the value whenever the value changes
	 * from outside - a pick in the calendar, a form reset - and is only read back on commit, so a
	 * half typed "22.0" never reaches the form as a date.
	 */
	const [text, setText] = useState(() => formatted(value));

	useEffect(() => {
		setText(formatted(value));
	}, [value, formatted]);

	const years = useMemo(() => {
		if (!yearSelect) {
			return undefined;
		}

		const now = new Date().getFullYear();

		return yearsBetween(
			yearRange?.from ?? minDate?.getFullYear() ?? now - DEFAULT_YEARS_BACK,
			yearRange?.to ?? maxDate?.getFullYear() ?? now + DEFAULT_YEARS_AHEAD,
		);
	}, [yearSelect, yearRange?.from, yearRange?.to, minDate, maxDate]);

	/**
	 * Takes what was typed. Something that is not a day, or a day outside the limits, goes back to
	 * the last good value rather than becoming an error to explain - the calendar is right there.
	 */
	const commitText = () => {
		const parsed = parseTypedDate(text);

		if (parsed === null) {
			if (clearable) {
				if (value) onChange?.(null);
			} else {
				setText(formatted(value));
			}

			return;
		}

		if (parsed === undefined || isDateDisabled(parsed, minDate, maxDate)) {
			setText(formatted(value));
			return;
		}

		const normalized = normalizeDate(parsed);

		if (!value || normalized.getTime() !== normalizeDate(value).getTime()) {
			onChange?.(normalized);
		}

		setText(formatted(normalized));
		setVisibleMonth(normalized);
	};

	const handleOpen = () => {
		if (disabled) {
			return;
		}

		if (value) {
			setVisibleMonth(normalizeDate(value));
		}

		setOpen((current) => !current);
	};

	const handleSelect = (date: Date) => {
		const normalized = normalizeDate(date);

		onChange?.(normalized);
		setVisibleMonth(normalized);
		setOpen(false);

		requestAnimationFrame(() => {
			inputRef.current?.focus();
		});
	};

	const handleClear = () => {
		onChange?.(null);
		setOpen(false);

		requestAnimationFrame(() => {
			inputRef.current?.focus();
		});
	};

	const handleToday = () => {
		const today = clampDate(new Date(), minDate, maxDate);

		onChange?.(today);
		setVisibleMonth(today);
		setOpen(false);

		requestAnimationFrame(() => {
			inputRef.current?.focus();
		});
	};

	const handleMonthChange = (month: Date) => {
		setVisibleMonth(month);
	};

	return (
		<div ref={rootRef} className={clsx("relative w-full", className)}>
			{name && (
				<input
					type="hidden"
					name={name}
					value={
						value
							? `${value.getFullYear()}-${String(value.getMonth() + 1).padStart(2, "0")}-${String(
									value.getDate(),
								).padStart(2, "0")}`
							: ""
					}
				/>
			)}

			<div
				className={clsx(
					"flex min-h-9.5 w-full items-center gap-2",
					"rounded-[3px]",
					"border",
					"bg-(--color-surface)",
					"px-3",
					"transition-colors",
					error ? "border-(--color-danger)" : "border-(--color-border)",
					!disabled && !error && "hover:border-(--color-border-strong)",
					!disabled && "focus-within:ring-2 focus-within:ring-(--color-primary-soft)",
					disabled && "cursor-not-allowed bg-(--color-surface-subtle) opacity-60",
				)}
			>
				<button
					type="button"
					tabIndex={-1}
					disabled={disabled}
					aria-label="Open calendar"
					aria-haspopup="dialog"
					aria-expanded={open}
					onClick={handleOpen}
					className="inline-flex shrink-0 items-center outline-none"
				>
					<CalendarDays size={16} strokeWidth={1.8} className="text-(--color-text-muted)" />
				</button>

				<input
					ref={inputRef}
					id={id}
					type="text"
					inputMode="numeric"
					autoComplete="off"
					disabled={disabled}
					aria-label={ariaLabel ?? placeholder}
					// The shape to type in; what the field is for is the label and the aria-label.
					placeholder="dd.mm.yyyy"
					value={text}
					onChange={(event) => setText(withDateSeparators(event.target.value, text))}
					onBlur={commitText}
					onKeyDown={(event) => {
						if (event.key === "Enter") {
							// Commit instead of submitting the form with a half typed date in it.
							event.preventDefault();
							commitText();
						}

						if (event.key === "ArrowDown" && !open) {
							event.preventDefault();
							handleOpen();
						}
					}}
					className="min-w-0 flex-1 bg-transparent text-sm text-(--color-text) outline-none placeholder:text-(--color-text-muted)"
				/>

				<button
					type="button"
					tabIndex={-1}
					disabled={disabled}
					aria-label={open ? "Close calendar" : "Open calendar"}
					onClick={handleOpen}
					className="inline-flex shrink-0 items-center outline-none"
				>
					<ChevronDown
						size={16}
						className={clsx("text-(--color-text-muted) transition-transform", open && "rotate-180")}
					/>
				</button>

				{value && clearable && !disabled && (
					<button
						type="button"
						aria-label="Clear date"
						onClick={handleClear}
						className="
					inline-flex
					h-6
					w-6
					shrink-0
					items-center
					justify-center
					rounded-[3px]
					text-(--color-text-muted)
					transition-colors
					hover:bg-(--color-surface-hover)
					hover:text-(--color-text)
				"
					>
						<X size={14} />
					</button>
				)}
			</div>

			{error && <div className="mt-1 text-xs text-(--color-danger)">{error}</div>}

			{open &&
				typeof document !== "undefined" &&
				createPortal(
					<div
						ref={popoverRef}
						// Above the dialogs, drawers and dropdowns, which all sit at 1000 - the field
						// this belongs to is often inside one of them.
						className="fixed z-[1100] max-w-[calc(100vw-16px)]"
						style={{
							top: position?.top ?? 0,
							left: position?.left ?? 0,
							// Hidden for the single frame before it has been measured, so it is never
							// seen in the wrong place.
							visibility: position ? "visible" : "hidden",
						}}
					>
						<DatePickerCalendar
							month={visibleMonth}
							selectedDate={value}
							minDate={minDate}
							maxDate={maxDate}
							locale={locale}
							onMonthChange={handleMonthChange}
							onSelect={handleSelect}
							onToday={handleToday}
							years={years}
						/>
					</div>,
					document.body,
				)}
		</div>
	);
}

export default DatePicker;
