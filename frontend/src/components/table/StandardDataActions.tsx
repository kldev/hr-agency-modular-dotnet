import { Pencil, Trash2 } from "lucide-react";

import { ActionButton } from "@/components/ui";

interface StandardDataActionsProps {
	onEdit: () => void;
	onDelete?: () => void;
}

export function StandardDataActions({ onEdit, onDelete }: StandardDataActionsProps) {
	return (
		<div className="standard-actions">
			<ActionButton title="Edit" onClick={onEdit}>
				<Pencil size={15} />
			</ActionButton>

			{onDelete ? (
				<ActionButton title="Delete" onClick={onDelete}>
					<Trash2 size={15} />
				</ActionButton>
			) : null}
		</div>
	);
}
