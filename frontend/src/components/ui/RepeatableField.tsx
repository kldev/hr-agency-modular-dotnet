import { Plus, Trash2 } from "lucide-react";
import type { ReactNode } from "react";
import { Button } from "./Button";

export type RepeatableFieldProps<T> = {
	label?: string;
	description?: string;
	items: T[];
	/** Renders the editable part of a row; the remove button is supplied around it. */
	renderRow: (item: T, index: number) => ReactNode;
	getRowKey: (item: T, index: number) => string;
	onAdd: () => void;
	onRemove: (index: number) => void;
	addLabel?: string;
	/** Rows down to this count cannot be removed - the button goes disabled instead of vanishing. */
	minItems?: number;
	canAdd?: boolean;
	disabled?: boolean;
};

/**
 * The object-shaped counterpart of ArrayField: a list of rows a form collects, each of them a small
 * composite rather than a single string. It owns the add/remove chrome and nothing about the row -
 * what a row contains is entirely up to `renderRow`, so the widget stays free of any one domain.
 */
export function RepeatableField<T>({
	label,
	description,
	items,
	renderRow,
	getRowKey,
	onAdd,
	onRemove,
	addLabel = "Add item",
	minItems = 0,
	canAdd = true,
	disabled,
}: RepeatableFieldProps<T>) {
	const rows = items ?? [];
	const canRemove = rows.length > minItems;

	return (
		<div>
			{label || description ? (
				<div className="mb-3">
					{label ? <h3 className="text-sm font-semibold">{label}</h3> : null}
					{description ? (
						<p className="mt-1 text-xs text-(--color-text-muted)">{description}</p>
					) : null}
				</div>
			) : null}

			<div className="space-y-2">
				{rows.map((item, index) => (
					<div key={getRowKey(item, index)} className="flex items-start gap-2">
						<div className="min-w-0 flex-1">{renderRow(item, index)}</div>

						<Button
							variant="ghost"
							isIconOnly
							className="action-button action-button-danger shrink-0"
							aria-label={`Remove row ${index + 1}`}
							title={canRemove ? "Remove" : `At least ${minItems} required`}
							isDisabled={disabled || !canRemove}
							onPress={() => onRemove(index)}
							icon={<Trash2 size={15} />}
						/>
					</div>
				))}

				<Button type="button" variant="secondary" isDisabled={disabled || !canAdd} onPress={onAdd}>
					<Plus size={15} />
					{addLabel}
				</Button>
			</div>
		</div>
	);
}
