import { ToggleButton } from "@heroui/react";
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

/*
 * A row of HeroUI toggle buttons, each controlled on its own rather than through a
 * ToggleButtonGroup: a single-selection group turns the buttons into radios, and the filters (and
 * the tests that read them) speak "pressed". Pressing the chosen one again keeps it chosen, as it
 * always did - clearing is what "All" is for.
 */
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
				<ToggleButton
					className="enum-filter-button"
					isSelected={value === null}
					onChange={() => onChange(null)}
				>
					{allLabel}
				</ToggleButton>
			)}

			{(Object.keys(options) as T[]).map((option) => (
				<ToggleButton
					key={option}
					className={clsx("enum-filter-button", `enum-filter-${option.toLowerCase()}`)}
					isSelected={value === option}
					onChange={() => onChange(option)}
				>
					{options[option]}
				</ToggleButton>
			))}
		</fieldset>
	);
}
