import { useNavigate } from "@tanstack/react-router";
import { MessageSquare, NotebookPen, Pencil, Settings2, TagPlus, TrendingUp } from "lucide-react";
import { ActionMenu } from "@/components/ui/ActionMenu";
import type { JobApplicationsActionsType } from "../forms";

interface AplicationsProps {
	id: string;
	onAction: (action: JobApplicationsActionsType) => void;
}

export function ApplicationsActions({ onAction, id }: AplicationsProps) {
	const navigate = useNavigate();

	return (
		<div className="table-actions">
			<ActionMenu
				actions={[
					{ label: "Edit", icon: Pencil, action: () => onAction("edit") },
					{
						label: "Open details",
						icon: Settings2,
						action: () => {
							navigate({
								to: "/app/applications/$id",
								params: { id },
								search: { status: undefined, search: undefined, source: undefined },
							});
						},
						dividerAfter: true,
					},
					{ label: "Add note", icon: NotebookPen, action: () => onAction("add-note") },
					{ label: "Change status", icon: TrendingUp, action: () => onAction("change-status") },
					{ label: "Add tag", icon: TagPlus, action: () => onAction("tag"), dividerAfter: true },
					{ label: "Schedule interview", icon: MessageSquare, action: () => onAction("schedule") },
				]}
			/>
		</div>
	);
}
