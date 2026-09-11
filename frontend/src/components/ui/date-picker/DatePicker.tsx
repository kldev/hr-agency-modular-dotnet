import clsx from "clsx";
import type { Locale } from "date-fns";
import { pl } from "date-fns/locale";
import { CalendarDays, ChevronDown, X } from "lucide-react";
import { useEffect, useRef, useState } from "react";
import { DatePickerCalendar } from "./DatePickerCalendar";
import { clampDate, normalizeDate } from "./datePickerUtils";

export interface DatePickerProps {
	value?: Date | null;
	onChange: (value: Date | null) => void;

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

	const [open, setOpen] = useState(false);

	const [visibleMonth, setVisibleMonth] = useState<Date>(normalizeDate(value ?? new Date()));

	useEffect(() => {
		if (value) {
			setVisibleMonth(normalizeDate(value));
		}
	}, [value]);

	useEffect(() => {
		if (!open) {
			return;
		}

		const handlePointerDown = (event: PointerEvent) => {
			const target = event.target as Node;

			if (rootRef.current && !rootRef.current.contains(target)) {
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

		onChange(normalized);
		setVisibleMonth(normalized);
		setOpen(false);

		requestAnimationFrame(() => {
			buttonRef.current?.focus();
		});
	};

	const handleClear = () => {
		onChange(null);
		setOpen(false);

		requestAnimationFrame(() => {
			buttonRef.current?.focus();
		});
	};

	const handleToday = () => {
		const today = clampDate(new Date(), minDate, maxDate);

		onChange(today);
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
			{/* Hidden native form value */}
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

			<button
				ref={buttonRef}
				id={id}
				type="button"
				disabled={disabled}
				aria-label={ariaLabel ?? placeholder}
				aria-haspopup="dialog"
				aria-expanded={open}
				onClick={handleOpen}
				className={clsx(
					"flex min-h-9.5 w-full items-center gap-2",
					"rounded-[3px]",
					"border",
					"bg-(--color-surface)",
					"px-3",
					"text-left",
					"transition-colors",

					error ? "border-(--color-danger)" : "border-(--color-border)",

					!disabled && !error && "hover:border-(--color-border-strong)",

					!disabled && "focus:outline-none focus:ring-2 focus:ring-(--color-primary-soft)",

					disabled && "cursor-not-allowed bg-(--color-surface-subtle) opacity-60",
				)}
			>
				<CalendarDays size={16} strokeWidth={1.8} className="shrink-0 text-(--color-text-muted)" />

				<span
					className={clsx(
						"min-w-0 flex-1 truncate text-sm",
						value ? "text-(--color-text)" : "text-(--color-text-muted)",
					)}
				>
					{formattedValue || placeholder}
				</span>

				{value && clearable && !disabled && (
					<button
						type="button"
						tabIndex={-1}
						aria-label="Clear date"
						onClick={(event) => {
							event.stopPropagation();
							handleClear();
						}}
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

				<ChevronDown
					size={16}
					className={clsx(
						"shrink-0 text-(--color-text-muted) transition-transform",
						open && "rotate-180",
					)}
				/>
			</button>

			{error && (
				<div
					className="
            mt-1
            text-xs
            text-(--color-danger)
          "
				>
					{error}
				</div>
			)}

			{open && (
				<div
					className="
            absolute
            left-0
            top-[calc(100%+6px)]
            z-50
            max-w-[calc(100vw-24px)]
          "
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
				</div>
			)}
		</div>
	);
}

export default DatePicker;
