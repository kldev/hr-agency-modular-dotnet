import clsx from "clsx";
import { useMemo, useState } from "react";

type EnumComboBoxProps<T extends string> = {
	value: T | null;
	onChange: (value: T | null) => void;
	options: Record<T, string>;
	placeholder?: string;
	allLabel?: string;
	className?: string;
};

export function EnumComboBox<T extends string>({
	value,
	onChange,
	options,
	placeholder = "Select...",
	allLabel = "All",
	className,
}: EnumComboBoxProps<T>) {
	const [query, setQuery] = useState("");
	const [open, setOpen] = useState(false);

	const filteredOptions = useMemo(() => {
		const normalizedQuery = query.toLowerCase().trim();

		return (Object.keys(options) as T[]).filter((option) => {
			if (!normalizedQuery) {
				return true;
			}

			return (
				option.toLowerCase().includes(normalizedQuery) ||
				options[option].toLowerCase().includes(normalizedQuery)
			);
		});
	}, [options, query]);

	const selectedLabel = value ? options[value] : "";

	return (
		<div className={clsx("enum-combobox", className)}>
			<input
				type="text"
				value={open ? query : selectedLabel}
				placeholder={value ? undefined : placeholder}
				onFocus={() => {
					setOpen(true);
					setQuery("");
				}}
				onChange={(event) => {
					setQuery(event.target.value);
					setOpen(true);
				}}
				onBlur={() => {
					setTimeout(() => setOpen(false), 150);
				}}
			/>

			{open && (
				<div className="enum-combobox-options">
					<button
						type="button"
						className={clsx("enum-combobox-option", value === null && "selected")}
						onMouseDown={(event) => event.preventDefault()}
						onClick={() => {
							onChange(null);
							setQuery("");
							setOpen(false);
						}}
					>
						{allLabel}
					</button>

					{filteredOptions.map((option) => (
						<button
							key={option}
							type="button"
							className={clsx("enum-combobox-option", value === option && "selected")}
							onMouseDown={(event) => event.preventDefault()}
							onClick={() => {
								onChange(option);
								setQuery("");
								setOpen(false);
							}}
						>
							{options[option]}
						</button>
					))}

					{filteredOptions.length === 0 && <div className="enum-combobox-empty">No results</div>}
				</div>
			)}
		</div>
	);
}
