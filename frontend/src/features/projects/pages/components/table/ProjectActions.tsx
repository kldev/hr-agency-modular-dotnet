import { useNavigate } from "@tanstack/react-router";
import { Pencil, Settings2, Shuffle } from "lucide-react";
import { ActionMenu } from "@/components/ui/ActionMenu";

interface ProjectActionsProps {
	id: string;
	onEdit: () => void;
	onChangeStatus: () => void;
}

export function ProjectActions({ id, onEdit, onChangeStatus }: ProjectActionsProps) {
	const navigate = useNavigate();

	return (
		<div className="table-actions">
			<ActionMenu
				actions={[
					{
						label: "Open details",
						icon: Settings2,
						action: () => {
							navigate({ to: "/app/projects/$id", params: { id }, search: { search: undefined } });
						},
					},
					{ label: "Edit", icon: Pencil, action: onEdit },
					{ label: "Change status", icon: Shuffle, action: onChangeStatus },
				]}
			/>
		</div>
	);
}
