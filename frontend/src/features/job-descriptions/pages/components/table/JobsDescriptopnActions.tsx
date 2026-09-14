import { useRouter } from "@tanstack/react-router";
import { Pencil, Settings2, TrendingUp } from "lucide-react";
import { ActionMenu } from "@/components/ui/ActionMenu";

interface JobsDescriptopnActionsProps {
	id: string;
	onChangeStatus: () => void;
}
export function JobsDescriptopnActions({ id, onChangeStatus }: JobsDescriptopnActionsProps) {
	const router = useRouter();
	const editUrl = router.buildLocation({
		to: "/app/job-descriptions/edit/$id",
		params: { id },
	}).href;

	const detailsUrl = router.buildLocation({
		to: "/app/job-descriptions/$id",
		params: { id },
	}).href;

	return (
		<div className="table-actions">
			<ActionMenu
				actions={[
					{
						label: "Edit",
						icon: Pencil,
						action: () => {
							window.open(editUrl, "_blank", "noopener,noreferrer");
						},
					},
					{
						label: "Open details",
						icon: Settings2,
						action: () => {
							window.open(detailsUrl, "_blank", "noopener,noreferrer");
						},
						dividerAfter: true,
					},
					{ label: "Change status", icon: TrendingUp, action: onChangeStatus },
				]}
			/>
		</div>
	);
}
