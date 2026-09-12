import clsx from "clsx";
import type { TextareaHTMLAttributes } from "react";

type TextareaProps = TextareaHTMLAttributes<HTMLTextAreaElement> & {
	variant?: "default" | "error";
};

export function Textarea({ className, variant = "default", ...props }: TextareaProps) {
	return <textarea {...props} className={clsx("textarea", `textarea-${variant}`, className)} />;
}
