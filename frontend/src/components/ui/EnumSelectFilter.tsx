import clsx from "clsx";
import { Select } from "./Select";

type EnumSelectFilterProps<T extends string> = {
	value: T | null;
	onChange: (value: T | null) => void;
	options: Record<T, string>;
	allLabel?: string;
	className?: string;
};

export function EnumSelectFilter<T extends string>({
	value,
	onChange,
	options,
	allLabel = "All",
	className,
}: EnumSelectFilterProps<T>) {
	return (
		<Select
			value={value ?? ""}
			className={clsx("enum-select-filter", className)}
			onChange={(event) => {
				const value = event.target.value;
				onChange(value === "" ? null : (value as T));
			}}
		>
			<option value="">{allLabel}</option>

			{(Object.keys(options) as T[]).map((option) => (
				<option key={option} value={option}>
					{options[option]}
				</option>
			))}
		</Select>
	);
}
