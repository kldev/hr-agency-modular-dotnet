import { Archive, ArchiveRestore, Pencil } from "lucide-react";
import { ActionMenu } from "@/components/ui/ActionMenu";

interface PositionActionsProps {
	isArchived: boolean;
	onEdit: () => void;
	onArchive: () => void;
	onRestore: () => void;
}

/**
 * There is no delete. A role somebody was posted onto is part of their contract, so it is archived:
 * gone from the picker and from the default list, still there in the history. Mirrors
 * `ProjectPositionArchived` — the backend has no remove command at all.
 */
export function PositionActions({
	isArchived,
	onEdit,
	onArchive,
	onRestore,
}: PositionActionsProps) {
	const actions = [
		{ label: "Edit", icon: Pencil, action: onEdit },
		isArchived
			? { label: "Restore", icon: ArchiveRestore, action: onRestore }
			: { label: "Archive", icon: Archive, action: onArchive },
	];

	return (
		<div className="table-actions">
			<ActionMenu actions={actions} />
		</div>
	);
}
