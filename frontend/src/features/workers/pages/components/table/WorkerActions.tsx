import { useNavigate } from "@tanstack/react-router";
import { CalendarPlus, Pencil, Settings2, Shuffle } from "lucide-react";
import { ActionMenu } from "@/components/ui/ActionMenu";

interface WorkerActionsProps {
	id: string;
	onEdit?: () => void;
	onChangeStatus?: () => void;
	onPlanAssignment?: () => void;
	mode?: "table" | "details";
}

/**
 * The same menu in the register and in the details header: on the details page "Open details" leads
 * where we already are and the header carries its own Edit button, so both drop out.
 *
 * Every action is optional and an absent one is simply not offered, which is how this menu stays
 * honest while the drawers behind it are still being built.
 */
export function WorkerActions({
	id,
	onEdit,
	onChangeStatus,
	onPlanAssignment,
	mode = "table",
}: WorkerActionsProps) {
	const navigate = useNavigate();

	const actions = [
		...(mode === "table"
			? [
					{
						label: "Open details",
						icon: Settings2,
						action: () => {
							navigate({
								to: "/app/workers/$id",
								params: { id },
								search: { search: undefined, tab: undefined },
							});
						},
					},
				]
			: []),
		...(mode === "table" && onEdit ? [{ label: "Edit", icon: Pencil, action: onEdit }] : []),
		...(onChangeStatus ? [{ label: "Change status", icon: Shuffle, action: onChangeStatus }] : []),
		...(onPlanAssignment
			? [{ label: "Plan assignment", icon: CalendarPlus, action: onPlanAssignment }]
			: []),
	];

	return (
		<div className="table-actions">
			<ActionMenu actions={actions} />
		</div>
	);
}
