import { Pencil, Settings2, TrendingUp } from "lucide-react";
import { ActionMenu } from "@/components/ui/ActionMenu";
import { RoutesNavigation } from "@/routes";

interface JobsDescriptopnActionsProps {
	id: string;
	onChangeStatus: () => void;
}
export function JobsDescriptopnActions({ id, onChangeStatus }: JobsDescriptopnActionsProps) {
	return (
		<div className="table-actions">
			<ActionMenu
				actions={[
					{
						label: "Edit",
						icon: Pencil,
						action: () => {
							window.open(
								RoutesNavigation.getJobsDescriptionEditPath(id),
								"_blank",
								"noopener,noreferrer",
							);
						},
					},
					{
						label: "Open details",
						icon: Settings2,
						action: () => {
							window.open(
								RoutesNavigation.getJobsDescriptionDetailsPath(id),
								"_blank",
								"noopener,noreferrer",
							);
						},
						dividerAfter: true,
					},
					{ label: "Change status", icon: TrendingUp, action: onChangeStatus },
				]}
			/>
		</div>
	);
}
