import { MessageSquare, NotebookPen, TagPlus, TrendingUp } from "lucide-react";

import { ActionMenu } from "@/components/ui/ActionMenu";
import type { JobApplicationsActionsType } from "../forms";

interface DetailsActionsProps {
	onAction: (action: JobApplicationsActionsType) => void;
}

export function DetailsActions({ onAction }: DetailsActionsProps) {
	return (
		<div className="table-actions">
			<ActionMenu
				actions={[
					{ label: "Add note", icon: NotebookPen, action: () => onAction("add-note") },
					{ label: "Change status", icon: TrendingUp, action: () => onAction("change-status") },
					{ label: "Add tag", icon: TagPlus, action: () => onAction("tag"), dividerAfter: true },
					{ label: "Schedule interview", icon: MessageSquare, action: () => onAction("schedule") },
				]}
			/>
		</div>
	);
}
