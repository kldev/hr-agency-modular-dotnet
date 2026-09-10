import type { InputHTMLAttributes } from "react";

export function Input({ className = "", ...props }: InputHTMLAttributes<HTMLInputElement>) {
	return (
		<input
			{...props}
			className={[
				"h-9 w-full rounded-md border border-(--color-border-strong) bg-(--color-surface) px-3 text-sm text-(--color-text)",
				"placeholder:text-(--color-text-muted) hover:border-(--color-border-strong)",
				"focus:border-(--color-primary) focus:ring-2 focus:ring-(--color-primary-soft)",
				className,
			].join(" ")}
		/>
	);
}
