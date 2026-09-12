import { Plus, Trash2 } from "lucide-react";
import { Button } from "./Button";

export function ArrayField({
	label,
	description,
	values,
	placeholder,
	onChange,
}: {
	label: string;
	description?: string;
	values: string[];
	placeholder?: string;
	onChange: (values: string[]) => void;
}) {
	const update = (index: number, value: string) =>
		onChange(values.map((item, i) => (i === index ? value : item)));

	const remove = (index: number) => {
		const next = values.filter((_, i) => i !== index);
		onChange(next.length ? next : [""]);
	};

	return (
		<div>
			<div className="mb-3">
				<h3 className="text-sm font-semibold">{label}</h3>
				{description && <p className="mt-1 text-xs text-(--color-text-muted)">{description}</p>}
			</div>

			<div className="space-y-2">
				{values.map((item, index) => (
					<div key={index} className="flex items-center gap-2">
						<input
							value={item}
							aria-label={`${label} ${index + 1}`}
							placeholder={placeholder}
							onChange={(event) => update(index, event.target.value)}
							className={[
								"h-9 min-w-0 flex-1 rounded-md border border-(--color-border-strong) bg-(--color-surface) px-3",
								"text-sm text-(--color-text) placeholder:text-(--color-text-muted) focus:border-(--color-primary)",
								" focus:ring-2 focus:ring-(--color-primary-soft)",
							].join(" ")}
						/>
						<button
							type="button"
							aria-label={`Remove ${label.toLowerCase()} ${index + 1}`}
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
				))}

				<Button type="button" variant="secondary" onClick={() => onChange([...values, ""])}>
					<Plus size={15} />
					Add item
				</Button>
			</div>
		</div>
	);
}
