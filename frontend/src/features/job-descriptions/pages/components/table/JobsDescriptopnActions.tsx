import { useRouter } from "@tanstack/react-router";
import { Pencil, Settings2, TrendingUp, UserRoundCog } from "lucide-react";
import { ActionMenu } from "@/components/ui/ActionMenu";

interface JobsDescriptopnActionsProps {
	id: string;
	onChangeStatus: () => void;
	onChangeRecruiter: () => void;
}
export function JobsDescriptopnActions({
	id,
	onChangeStatus,
	onChangeRecruiter,
}: JobsDescriptopnActionsProps) {
	const router = useRouter();
	const editUrl = router.buildLocation({
		to: "/app/job-descriptions/edit/$id",
		params: { id },
	}).href;

	const detailsUrl = router.buildLocation({
		to: "/app/job-descriptions/$id",
		params: { id },
		search: { search: undefined, status: undefined },
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
					{ label: "Change recruiter", icon: UserRoundCog, action: onChangeRecruiter },
				]}
			/>
		</div>
	);
}
