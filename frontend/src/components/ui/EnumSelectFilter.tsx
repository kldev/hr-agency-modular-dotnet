import clsx from "clsx";
import { Select } from "./Select";

type EnumSelectFilterProps<T extends string> = {
	id?: string;
	/** For a select with no visible label of its own, like one per row of a repeatable field. */
	"aria-label"?: string;
	value: T | null;
	onChange: (value: T | null | undefined) => void;
	options: Record<T, string>;
	allLabel?: string;
	/** Shown while nothing is picked, when there is no "all" entry to stand in for it. */
	placeholder?: string;
	className?: string;
	hideAll?: boolean;
};

export function EnumSelectFilter<T extends string>({
	id,
	"aria-label": ariaLabel,
	value,
	onChange,
	options,
	allLabel = "All",
	placeholder = "Select…",
	className,
	hideAll,
}: EnumSelectFilterProps<T>) {
	/*
	 * A select whose value is empty still has to carry an empty option, or the browser shows the
	 * first real one and the field reads as chosen while the form holds nothing - which is how a
	 * required field ends up displaying an answer next to "Pick a status".
	 *
	 * Not rendered when the caller already supplies an option with an empty value (the compliance
	 * proof picker's "No document"), because two options sharing a value make the first one win.
	 */
	const hasEmptyOption = Object.hasOwn(options, "");
	const showPlaceholder = hideAll && !hasEmptyOption && !value;

	return (
		<Select
			id={id}
			aria-label={ariaLabel}
			value={value ?? ""}
			className={clsx("enum-select-filter", className)}
			onChange={(event) => {
				const value = event.target.value;
				onChange(value === "" ? null : (value as T));
			}}
		>
			{hideAll ? null : <option value="">{allLabel}</option>}

			{showPlaceholder ? <option value="">{placeholder}</option> : null}

			{(Object.keys(options) as T[]).map((option) => (
				<option key={option} value={option}>
					{options[option]}
				</option>
			))}
		</Select>
	);
}
