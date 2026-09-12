import { NotebookPen, Pencil, Settings2, TagPlus, TrendingUp } from "lucide-react";
import { useNavigate } from "react-router-dom";
import { ActionMenu } from "@/components/ui/ActionMenu";
import { RoutesNavigation } from "@/routes";

interface AplicationsProps {
	id: string;
	onEdit: () => void;
	onChangeStatus: () => void;
	addNote: () => void;
	addTag: () => void;
}

export function AplicationsActions({
	onEdit,
	onChangeStatus,
	addNote,
	addTag,
	id,
}: AplicationsProps) {
	const navigate = useNavigate();

	return (
		<div className="table-actions">
			<ActionMenu
				actions={[
					{ label: "Edit", icon: Pencil, action: onEdit },
					{
						label: "Open details",
						icon: Settings2,
						action: () => {
							navigate(RoutesNavigation.getApplicationPath(id));
						},
						dividerAfter: true,
					},
					{ label: "Add note", icon: NotebookPen, action: addNote },
					{ label: "Change status", icon: TrendingUp, action: onChangeStatus },
					{ label: "Add tag", icon: TagPlus, action: addTag },
				]}
			/>
		</div>
	);
}
