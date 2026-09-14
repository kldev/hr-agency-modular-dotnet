import { useNavigate } from "@tanstack/react-router";
import { Pencil, Settings2 } from "lucide-react";
import { ActionMenu } from "@/components/ui/ActionMenu";

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
							navigate({ to: "/app/candidates/$id", params: { id } });
						},
					},
				]}
			/>
		</div>
	);
}
