import clsx from "clsx";
import type { InputHTMLAttributes } from "react";

export type InputProps = InputHTMLAttributes<HTMLInputElement> & {
	variant?: "default" | "error";
};

export function Input({ className, variant = "default", ...props }: InputProps) {
	return <input {...props} className={clsx("input", `input-${variant}`, className)} />;
}

/**
 * The money field types with a decimal comma, the way it is written here, and `Number` reads only
 * a dot - so "42,40" parsed directly is `NaN`, which fails every range check with a misleading
 * message. Everything that reads or fills a `MoneyInput` goes through these two.
 */
export function parseMoney(value: string): number {
	return Number(value.replace(",", "."));
}

/** The reverse, for filling the field from the API: 42.4 becomes "42,4", which the field accepts. */
export function moneyInputValue(amount: number | string | null | undefined): string {
	return amount === null || amount === undefined || amount === ""
		? ""
		: String(amount).replace(".", ",");
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
