import type { InputHTMLAttributes } from "react";

export type ToggleProps = Omit<InputHTMLAttributes<HTMLInputElement>, "type">;

export function Toggle({ checked = false, ...props }: ToggleProps) {
	return (
		<label className="toggle">
			<input {...props} type="checkbox" checked={checked} />
			<span className="toggle-slider" aria-hidden="true" />
		</label>
	);
}
