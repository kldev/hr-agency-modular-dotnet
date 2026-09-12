import type { InputHTMLAttributes } from "react";

type ToggleProps = Omit<InputHTMLAttributes<HTMLInputElement>, "type">;

export function Toggle(props: ToggleProps) {
	return (
		<label className="toggle">
			<input type="checkbox" {...props} />
			<span className="toggle-slider" aria-hidden="true" />
		</label>
	);
}
