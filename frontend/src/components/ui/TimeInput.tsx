import clsx from "clsx";
import { Check, ChevronDown, Clock } from "lucide-react";
import { useEffect, useRef, useState } from "react";

interface TimeInputProps {
	value?: string;
	onChange: (value: string) => void;
	onBlur?: () => void;
	disabled?: boolean;
	error?: string;
	id?: string;
	name?: string;
}

const START_MINUTES = 8 * 60;
const END_MINUTES = 22 * 60;
const STEP = 5;

function createTimeOptions(): string[] {
	const options: string[] = [];

	for (let minutes = START_MINUTES; minutes <= END_MINUTES; minutes += STEP) {
		const hours = Math.floor(minutes / 60);
		const mins = minutes % 60;

		options.push(`${String(hours).padStart(2, "0")}:${String(mins).padStart(2, "0")} `);
	}

	return options;
}

const TIME_OPTIONS = createTimeOptions();

export function TimeInput({
	value = "",
	onChange,
	onBlur,
	disabled = false,
	error,
	id,
	name,
}: TimeInputProps) {
	const rootRef = useRef<HTMLDivElement>(null);
	const [open, setOpen] = useState(false);

	useEffect(() => {
		if (!open) {
			return;
		}

		const handlePointerDown = (event: PointerEvent) => {
			const target = event.target as Node;

			if (rootRef.current && !rootRef.current.contains(target)) {
				setOpen(false);
				onBlur?.();
			}
		};

		const handleKeyDown = (event: KeyboardEvent) => {
			if (event.key === "Escape") {
				setOpen(false);
				onBlur?.();
			}
		};

		document.addEventListener("pointerdown", handlePointerDown);
		document.addEventListener("keydown", handleKeyDown);

		return () => {
			document.removeEventListener("pointerdown", handlePointerDown);
			document.removeEventListener("keydown", handleKeyDown);
		};
	}, [open, onBlur]);

	const handleSelect = (time: string) => {
		onChange(time);
		setOpen(false);
		onBlur?.();
	};

	return (
		<div ref={rootRef} className="relative w-full">
			{name && <input type="hidden" name={name} value={value} />}

			<button
				id={id}
				type="button"
				disabled={disabled}
				aria-haspopup="listbox"
				aria-expanded={open}
				onClick={() => setOpen((current) => !current)}
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
				<Clock size={16} strokeWidth={1.8} className="shrink-0 text-(--color-text-muted)" />

				<span
					className={clsx(
						"min-w-0 flex-1 text-sm",
						value ? "text-(--color-text)" : "text-(--color-text-muted)",
					)}
				>
					{value || "Select time"}
				</span>

				<ChevronDown
					size={16}
					className={clsx(
						"shrink-0 text-(--color-text-muted) transition-transform",
						open && "rotate-180",
					)}
				/>
			</button>

			{error && <div className="mt-1 text-xs text-(--color-danger)">{error}</div>}

			{open && (
				<div
					className="
						absolute
						left-0
						top-[calc(100%+6px)]
						z-50
						w-full
						min-w-32
						overflow-hidden
						rounded-[3px]
						border
						border-(--color-border)
						bg-(--color-surface)
						shadow-lg
					"
				>
					<div role="listbox" aria-label="Select time" className="max-h-60 overflow-y-auto py-1">
						{TIME_OPTIONS.map((time) => {
							const selected = time === value;

							return (
								<button
									key={time}
									type="button"
									role="option"
									aria-selected={selected}
									onClick={() => handleSelect(time)}
									className={clsx(
										"flex w-full items-center gap-2",
										"px-3 py-1.5",
										"text-left text-sm",
										"transition-colors",
										selected
											? "bg-(--color-primary-soft) text-(--color-primary)"
											: "text-(--color-text) hover:bg-(--color-surface-hover)",
									)}
								>
									<span className="w-4 shrink-0">{selected && <Check size={14} />}</span>

									<span>{time}</span>
								</button>
							);
						})}
					</div>
				</div>
			)}
		</div>
	);
}

export default TimeInput;
