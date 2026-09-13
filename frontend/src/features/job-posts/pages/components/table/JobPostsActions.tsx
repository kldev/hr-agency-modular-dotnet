import { ClipboardList, Pencil, Rss, Settings2, TrendingUp } from "lucide-react";
import { ActionMenu } from "@/components/ui/ActionMenu";
import { RoutesNavigation } from "@/routes";

interface JobPostsActionsProps {
	id: string;
	onAddApplication: () => void;
	onChangeStatus: () => void;
	onPostToChannel: () => void;
}
export function JobPostsActions({
	id,
	onAddApplication,
	onChangeStatus,
	onPostToChannel,
}: JobPostsActionsProps) {
	return (
		<div className="table-actions">
			<ActionMenu
				actions={[
					{
						label: "Edit",
						icon: Pencil,
						action: () => {
							window.open(RoutesNavigation.getJobsEdit(id), "_blank", "noopener,noreferrer");
						},
					},
					{ label: "Change status", icon: TrendingUp, action: onChangeStatus },
					{ label: "Post to channel", icon: Rss, action: onPostToChannel },
					{
						label: "Open details",
						icon: Settings2,
						action: () => {
							window.open(RoutesNavigation.getJobsDetailsPath(id), "_blank", "noopener,noreferrer");
						},
						dividerAfter: true,
					},
					{ label: "Add job application", icon: ClipboardList, action: onAddApplication },
				]}
			/>
		</div>
	);
}
