import { useNavigate } from "@tanstack/react-router";
import { Pencil, Settings2, TagPlus } from "lucide-react";
import { ActionMenu } from "@/components/ui/ActionMenu";

interface CandidateActionsProps {
	id: string;
	onEdit: () => void;
	onTag: () => void;
}

export function CandidateActions({ onEdit, id, onTag }: CandidateActionsProps) {
	const navigate = useNavigate();

	return (
		<div className="table-actions">
			<ActionMenu
				actions={[
					{ label: "Edit", icon: Pencil, action: () => onEdit() },
					{ label: "Add tag", icon: TagPlus, action: () => onTag(), dividerAfter: true },
					{
						label: "Open details",
						icon: Settings2,
						action: () => {
							navigate({
								to: "/app/candidates/$id",
								params: { id },
								search: { search: undefined, source: undefined },
							});
						},
					},
				]}
			/>
		</div>
	);
}
