import { Plus, Trash2 } from "lucide-react";
import { Button } from "./Button";

export type ArrayFieldProps = {
	label?: string;
	/**
	 * Names the rows and the group for assistive tech ("Skills 2") when the visible heading comes
	 * from elsewhere - a form wrapper that renders its own label. Defaults to `label`, then "Item".
	 */
	itemLabel?: string;
	description?: string;
	values: string[];
	placeholder?: string;
	/** Rows shorter than this are marked right away, without waiting for schema validation. */
	minLength?: number;
	disabled?: boolean;
	onChange: (values: string[]) => void;
};

export function ArrayField({
	label,
	itemLabel = label || "Item",
	description,
	values,
	placeholder,
	minLength,
	disabled,
	onChange,
}: ArrayFieldProps) {
	const items = values ?? [];

	const update = (index: number, value: string) =>
		onChange(items.map((item, i) => (i === index ? value : item)));

	/*
	 * Trimming happens on blur, not on change - trimming while typing would eat the space between
	 * two words. It keeps whitespace-only rows from reaching the form value.
	 */
	const normalize = (index: number) => {
		const current = items[index] ?? "";
		const trimmed = current.trim();

		if (trimmed === current) {
			return;
		}

		update(index, trimmed);
	};

	const remove = (index: number) => {
		const next = items.filter((_, i) => i !== index);
		onChange(next.length ? next : [""]);
	};

	const lastItem = items.at(-1) ?? "";
	const canAdd = items.length === 0 || lastItem.trim().length > 0;

	return (
		<fieldset aria-label={itemLabel} className="min-w-0">
			<div className="mb-3">
				{label ? <h3 className="text-sm font-semibold">{label}</h3> : null}
				{description && <p className="mt-1 text-xs text-(--color-text-muted)">{description}</p>}
			</div>

			<div className="space-y-2">
				{items.map((item, index) => {
					const value = item.trim();
					const tooShort = minLength !== undefined && value.length > 0 && value.length < minLength;

					return (
						<div key={index}>
							<div className="flex items-center gap-2">
								<input
									value={item}
									aria-label={`${itemLabel} ${index + 1}`}
									aria-invalid={tooShort}
									placeholder={placeholder}
									disabled={disabled}
									onChange={(event) => update(index, event.target.value)}
									onBlur={() => normalize(index)}
									className={[
										"h-9 min-w-0 flex-1 rounded-md border bg-(--color-surface) px-3",
										tooShort ? "border-(--color-danger)" : "border-(--color-border-strong)",
										"text-sm text-(--color-text) placeholder:text-(--color-text-muted) focus:border-(--color-primary)",
										" focus:ring-2 focus:ring-(--color-primary-soft)",
									].join(" ")}
								/>
								<button
									type="button"
									aria-label={`Remove ${itemLabel?.toLowerCase()} ${index + 1}`}
									disabled={disabled}
									onClick={() => remove(index)}
									className={[
										"flex h-9 w-9 shrink-0 items-center justify-center",
										"rounded-md text-(--color-text-muted)",
										"hover:bg-(--color-danger-soft) hover:text-(--color-danger)",
									].join(" ")}
								>
									<Trash2 size={15} />
								</button>
							</div>

							{tooShort && (
								<p className="mt-1 text-xs text-(--color-danger)">
									At least {minLength} characters.
								</p>
							)}
						</div>
					);
				})}

				<Button
					type="button"
					variant="secondary"
					disabled={disabled || !canAdd}
					onClick={() => onChange([...items, ""])}
				>
					<Plus size={15} />
					Add item
				</Button>
			</div>
		</fieldset>
	);
}
