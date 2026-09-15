import clsx from "clsx";
import type { ChangeEvent, InputHTMLAttributes } from "react";
import { Input } from "./Input";

type WebsiteInputProps = Omit<InputHTMLAttributes<HTMLInputElement>, "type">;

function normalizeWebsite(value: string): string {
	const trimmed = value.trim();

	if (!trimmed) {
		return trimmed;
	}

	if (/^https?:\/\//i.test(trimmed)) {
		return trimmed;
	}

	return `http://${trimmed}`;
}

export function WebsiteInput({
	className,
	onPaste,
	onBlur,
	onChange,
	...props
}: WebsiteInputProps) {
	const handleChange = (event: ChangeEvent<HTMLInputElement>) => {
		onChange?.(event);
	};

	const handlePaste = (event: React.ClipboardEvent<HTMLInputElement>) => {
		onPaste?.(event);

		if (event.defaultPrevented) {
			return;
		}

		const pasted = event.clipboardData.getData("text");

		if (!pasted.trim()) {
			return;
		}

		event.preventDefault();

		const input = event.currentTarget;
		const value = normalizeWebsite(pasted);

		const start = input.selectionStart ?? input.value.length;
		const end = input.selectionEnd ?? input.value.length;

		const newValue = input.value.slice(0, start) + value + input.value.slice(end);

		const nativeSetter = Object.getOwnPropertyDescriptor(HTMLInputElement.prototype, "value")?.set;

		nativeSetter?.call(input, newValue);

		input.dispatchEvent(new Event("input", { bubbles: true }));
	};

	const handleBlur = (event: React.FocusEvent<HTMLInputElement>) => {
		const normalized = normalizeWebsite(event.currentTarget.value);

		if (normalized !== event.currentTarget.value) {
			const nativeSetter = Object.getOwnPropertyDescriptor(
				HTMLInputElement.prototype,
				"value",
			)?.set;

			nativeSetter?.call(event.currentTarget, normalized);

			event.currentTarget.dispatchEvent(new Event("input", { bubbles: true }));
		}

		onBlur?.(event);
	};

	return (
		<Input
			{...props}
			type="url"
			className={clsx(className)}
			onChange={handleChange}
			onPaste={handlePaste}
			onBlur={handleBlur}
		/>
	);
}
