import clsx from "clsx";

type EnumFilterProps<T extends string> = {
	value: T | null;
	onChange: (value: T | null) => void;
	options: Record<T, string>;
	allLabel?: string;
	/** For a choice that always has an answer, like a period: no "All" button. */
	hideAll?: boolean;
	className?: string;
};

export function EnumFilter<T extends string>({
	value,
	onChange,
	options,
	allLabel = "All",
	hideAll = false,
	className,
}: EnumFilterProps<T>) {
	return (
		<fieldset className={clsx("enum-filter", className)}>
			{hideAll ? null : (
				<button
					type="button"
					className={clsx("enum-filter-button", {
						"is-selected": value === null,
					})}
					aria-pressed={value === null}
					onClick={() => onChange(null)}
				>
					{allLabel}
				</button>
			)}

			{(Object.keys(options) as T[]).map((option) => (
				<button
					key={option}
					type="button"
					className={clsx("enum-filter-button", `enum-filter-${option.toLowerCase()}`, {
						"is-selected": value === option,
					})}
					aria-pressed={value === option}
					onClick={() => onChange(option)}
				>
					{options[option]}
				</button>
			))}
		</fieldset>
	);
}
