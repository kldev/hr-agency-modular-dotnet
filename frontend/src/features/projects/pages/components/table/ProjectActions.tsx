import { useNavigate } from "@tanstack/react-router";
import { Pencil, Settings2, Shuffle } from "lucide-react";
import { ActionMenu } from "@/components/ui/ActionMenu";

interface ProjectActionsProps {
	id: string;
	onEdit: () => void;
	onChangeStatus: () => void;
	mode?: "table" | "details";
}

/**
 * The same menu in the table and in the details header. On the details page "Open details" would
 * lead where we already are, and the header has its own Edit button - so both drop out and only the
 * status change is left, which is exactly what used to be a label crammed into a 30x30 icon button.
 */
export function ProjectActions({
	id,
	onEdit,
	onChangeStatus,
	mode = "table",
}: ProjectActionsProps) {
	const navigate = useNavigate();

	const actions =
		mode === "details"
			? [{ label: "Change status", icon: Shuffle, action: onChangeStatus }]
			: [
					{
						label: "Open details",
						icon: Settings2,
						action: () => {
							navigate({
								to: "/app/projects/$id",
								params: { id },
								search: { search: undefined, tab: undefined },
							});
						},
					},
					{ label: "Edit", icon: Pencil, action: onEdit },
					{ label: "Change status", icon: Shuffle, action: onChangeStatus },
				];

	return (
		<div className="table-actions">
			<ActionMenu actions={actions} />
		</div>
	);
}
