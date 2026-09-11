import clsx from "clsx";
import type { SelectHTMLAttributes } from "react";
import { europeanCountries } from "../types/europeanCountries";
import { Select } from "./Select";

export function CountrySelect({
	value,
	onChange,
	className,
	...props
}: SelectHTMLAttributes<HTMLSelectElement>) {
	return (
		<Select
			{...props}
			value={value}
			onChange={onChange}
			className={clsx("country-select", className)}
		>
			<option value="">Select country</option>

			{Object.entries(europeanCountries).map(([code, name]) => (
				<option key={code} value={code}>
					{code} — {name}
				</option>
			))}
		</Select>
	);
}
