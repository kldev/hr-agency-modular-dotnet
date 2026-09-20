import { useNavigate } from "@tanstack/react-router";
import { Pencil, Settings2, UserPlus } from "lucide-react";
import { ActionMenu } from "@/components/ui/ActionMenu";

interface TeamActionsProps {
	id: string;
	onRename: () => void;
	onAddMember: () => void;
}

export function TeamActions({ id, onRename, onAddMember }: TeamActionsProps) {
	const navigate = useNavigate();

	return (
		<div className="table-actions">
			<ActionMenu
				actions={[
					{ label: "Rename", icon: Pencil, action: onRename },
					{ label: "Add member", icon: UserPlus, action: onAddMember },
					{
						label: "Open details",
						icon: Settings2,
						action: () => {
							navigate({ to: "/app/teams/$id", params: { id }, search: { search: "" } });
						},
					},
				]}
			/>
		</div>
	);
}
