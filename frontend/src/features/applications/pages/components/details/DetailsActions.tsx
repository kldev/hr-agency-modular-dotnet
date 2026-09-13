import { MessageSquare, NotebookPen, TagPlus, TrendingUp } from "lucide-react";

import { ActionMenu } from "@/components/ui/ActionMenu";

interface DetailsActionsProps {
	onChangeStatus: () => void;
	addNote: () => void;
	addTag: () => void;
	scheduleInterview: () => void;
}

export function DetailsActions({
	onChangeStatus,
	addNote,
	addTag,

	scheduleInterview,
}: DetailsActionsProps) {
	return (
		<div className="table-actions">
			<ActionMenu
				actions={[
					{ label: "Add note", icon: NotebookPen, action: addNote },
					{ label: "Change status", icon: TrendingUp, action: onChangeStatus },
					{ label: "Add tag", icon: TagPlus, action: addTag, dividerAfter: true },
					{ label: "Schedule interview", icon: MessageSquare, action: scheduleInterview },
				]}
			/>
		</div>
	);
}
