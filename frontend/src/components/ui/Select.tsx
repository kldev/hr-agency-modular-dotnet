import clsx from "clsx";
import type { SelectHTMLAttributes } from "react";

type SelectProps = SelectHTMLAttributes<HTMLSelectElement>;

export function Select({ className, children, ...props }: SelectProps) {
	return (
		<select {...props} className={clsx("select", className)}>
			{children}
		</select>
	);
}
