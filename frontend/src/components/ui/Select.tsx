import clsx from "clsx";
import type { SelectHTMLAttributes } from "react";

type SelectProps = SelectHTMLAttributes<HTMLSelectElement>;

/*
 * A native <select> for the screens that still list their own <option>s; enums go through
 * EnumSelectFilter, which is HeroUI's Select. The class is not "select": HeroUI puts that on the
 * root of its own Select, and the panel's field styles would turn that wrapper into a box.
 */
export function Select({ className, children, ...props }: SelectProps) {
	return (
		<select {...props} className={clsx("native-select", className)}>
			{children}
		</select>
	);
}
