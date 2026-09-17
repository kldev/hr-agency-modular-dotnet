import clsx from "clsx";
import type { SelectHTMLAttributes } from "react";
import { jobLanguages } from "../types/languages";
import { Select } from "./Select";

export function LanguageSelect({
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
			className={clsx("language-select", className)}
		>
			<option value="">Select language</option>

			{Object.entries(jobLanguages).map(([code, name]) => (
				<option key={code} value={code}>
					{code} — {name}
				</option>
			))}
		</Select>
	);
}
