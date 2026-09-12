import { Pencil, Settings2 } from "lucide-react";
import { useNavigate } from "react-router-dom";
import { ActionMenu } from "@/components/ui/ActionMenu";
import { RoutesNavigation } from "@/routes";

interface CandidateActionsProps {
	id: string;
	onEdit: () => void;
}

export function CandidateActions({ onEdit, id }: CandidateActionsProps) {
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
							navigate(RoutesNavigation.getCandidatePath(id));
						},
					},
				]}
			/>
		</div>
	);
}
