import { useNavigate } from "@tanstack/react-router";
import { Pencil, Settings2, Shuffle } from "lucide-react";
import { ActionMenu } from "@/components/ui/ActionMenu";

interface AssignmentActionsProps {
	id: string;
	onEdit?: () => void;
	onChangeStatus?: () => void;
	mode?: "table" | "details";
}

/**
 * The same menu in the list and in the details header, as with workers: on the details page "Open
 * details" leads where we already are and the header carries its own Edit button, so both drop out.
 *
 * There is deliberately no "Move to another project": the backend has no command that repoints an
 * assignment, because a move is this one ending and another one opening.
 */
export function AssignmentActions({
	id,
	onEdit,
	onChangeStatus,
	mode = "table",
}: AssignmentActionsProps) {
	const navigate = useNavigate();

	const actions = [
		...(mode === "table"
			? [
					{
						label: "Open details",
						icon: Settings2,
						action: () => {
							navigate({
								to: "/app/assignments/$id",
								params: { id },
								search: { search: undefined },
							});
						},
					},
				]
			: []),
		...(mode === "table" && onEdit ? [{ label: "Edit", icon: Pencil, action: onEdit }] : []),
		...(onChangeStatus ? [{ label: "Change status", icon: Shuffle, action: onChangeStatus }] : []),
	];

	return (
		<div className="table-actions">
			<ActionMenu actions={actions} />
		</div>
	);
}
