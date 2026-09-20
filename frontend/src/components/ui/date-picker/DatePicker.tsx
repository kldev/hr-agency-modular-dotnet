import clsx from "clsx";
import type { Locale } from "date-fns";
import { pl } from "date-fns/locale";
import { CalendarDays, ChevronDown, X } from "lucide-react";
import { useCallback, useEffect, useLayoutEffect, useRef, useState } from "react";
import { createPortal } from "react-dom";
import { DatePickerCalendar } from "./DatePickerCalendar";
import { clampDate, normalizeDate } from "./datePickerUtils";

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
}: DatePickerProps) {
	const rootRef = useRef<HTMLDivElement>(null);
	const buttonRef = useRef<HTMLButtonElement>(null);
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
					buttonRef.current?.focus();
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
			buttonRef.current?.focus();
		});
	};

	const handleClear = () => {
		onChange?.(null);
		setOpen(false);

		requestAnimationFrame(() => {
			buttonRef.current?.focus();
		});
	};

	const handleToday = () => {
		const today = clampDate(new Date(), minDate, maxDate);

		onChange?.(today);
		setVisibleMonth(today);
		setOpen(false);

		requestAnimationFrame(() => {
			buttonRef.current?.focus();
		});
	};

	const handleMonthChange = (month: Date) => {
		setVisibleMonth(month);
	};

	const formattedValue = value
		? new Intl.DateTimeFormat(locale.code, {
				day: "2-digit",
				month: "2-digit",
				year: "numeric",
			}).format(value)
		: "";

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
					ref={buttonRef}
					id={id}
					type="button"
					disabled={disabled}
					aria-label={ariaLabel ?? placeholder}
					aria-haspopup="dialog"
					aria-expanded={open}
					onClick={handleOpen}
					className="flex min-w-0 flex-1 items-center gap-2 text-left outline-none"
				>
					<CalendarDays
						size={16}
						strokeWidth={1.8}
						className="shrink-0 text-(--color-text-muted)"
					/>

					<span
						className={clsx(
							"min-w-0 flex-1 truncate text-sm",
							value ? "text-(--color-text)" : "text-(--color-text-muted)",
						)}
					>
						{formattedValue || placeholder}
					</span>

					<ChevronDown
						size={16}
						className={clsx(
							"shrink-0 text-(--color-text-muted) transition-transform",
							open && "rotate-180",
						)}
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
						/>
					</div>,
					document.body,
				)}
		</div>
	);
}

export default DatePicker;
