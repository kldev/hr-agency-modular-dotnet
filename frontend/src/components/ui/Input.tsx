import clsx from "clsx";
import type { InputHTMLAttributes } from "react";

type InputProps = InputHTMLAttributes<HTMLInputElement> & {
	variant?: "default" | "error";
};

export function Input({ className, variant = "default", ...props }: InputProps) {
	return <input {...props} className={clsx("input", `input-${variant}`, className)} />;
}
