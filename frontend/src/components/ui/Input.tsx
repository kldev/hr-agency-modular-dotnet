import clsx from "clsx";
import type { InputHTMLAttributes } from "react";

export type InputProps = InputHTMLAttributes<HTMLInputElement> & {
	variant?: "default" | "error";
};

export function Input({ className, variant = "default", ...props }: InputProps) {
	return <input {...props} className={clsx("input", `input-${variant}`, className)} />;
}

export type MoneyInputProps = Omit<InputHTMLAttributes<HTMLInputElement>, "type"> & {
	variant?: "default" | "error";
};

export function MoneyInput({
	className,
	variant = "default",
	onChange,
	...props
}: MoneyInputProps) {
	return (
		<input
			{...props}
			type="text"
			inputMode="decimal"
			className={clsx("input", `input-${variant}`, className)}
			onChange={(event) => {
				const value = event.target.value;

				// cyfry + opcjonalny przecinek + maks. 4 cyfry po przecinku
				if (!/^\d*(,\d{0,4})?$/.test(value)) {
					return;
				}

				onChange?.(event);
			}}
		/>
	);
}
