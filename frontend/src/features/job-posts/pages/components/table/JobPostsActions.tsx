import { useRouter } from "@tanstack/react-router";
import {
	ClipboardList,
	Languages,
	Pencil,
	Rss,
	Settings2,
	TrendingUp,
	UserRoundCog,
} from "lucide-react";
import { ActionMenu } from "@/components/ui/ActionMenu";

interface JobPostsActionsProps {
	id: string;
	onAddApplication: () => void;
	onChangeStatus: () => void;
	onPostToChannel: () => void;
	onChangeRecruiter: () => void;
}
export function JobPostsActions({
	id,
	onAddApplication,
	onChangeStatus,
	onPostToChannel,
	onChangeRecruiter,
}: JobPostsActionsProps) {
	const router = useRouter();
	const detailsUrl = router.buildLocation({
		to: "/app/jobs/$id",
		params: { id: id },
		search: { search: "", status: undefined },
	}).href;

	const editUrl = router.buildLocation({
		to: "/app/jobs/edit/$id",
		params: { id: id },
	}).href;

	/* The copy is a translation into another language, so it starts from this post, not from scratch. */
	const copyUrl = router.buildLocation({
		to: "/app/jobs/add",
		search: { fromJobPostId: id, jobDescriptionId: undefined },
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
						label: "Copy to new language",
						icon: Languages,
						action: () => {
							window.open(copyUrl, "_blank", "noopener,noreferrer");
						},
					},
					{ label: "Change status", icon: TrendingUp, action: onChangeStatus },
					{ label: "Change recruiter", icon: UserRoundCog, action: onChangeRecruiter },
					{ label: "Post to channel", icon: Rss, action: onPostToChannel },
					{
						label: "Open details",
						icon: Settings2,
						action: () => {
							window.open(detailsUrl, "_blank", "noopener,noreferrer");
						},
						dividerAfter: true,
					},
					{ label: "Add job application", icon: ClipboardList, action: onAddApplication },
				]}
			/>
		</div>
	);
}
