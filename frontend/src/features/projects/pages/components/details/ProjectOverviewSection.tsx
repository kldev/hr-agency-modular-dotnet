import { UsersRound } from "lucide-react";
import type { ProjectProjection } from "@/api/models";
import { Button, DetailItem, DetailOverviewHeader } from "@/components/ui";
import { formatDate } from "@/utlis/dateUtils";
import { engagementTypes } from "../../../types";
import { formatAddress } from "../../../utils";

interface ProjectOverviewSectionProps {
	project: ProjectProjection;
	onAssignTeam: () => void;
}

export function ProjectOverviewSection({ project, onAssignTeam }: ProjectOverviewSectionProps) {
	return (
		<div className="data-overview">
			<DetailOverviewHeader
				title="Overview"
				description="What the engagement is, where it runs and for how long."
			/>

			<dl className="data-details-list">
				<DetailItem label="Engagement type">{engagementTypes[project.engagementType]}</DetailItem>

				<DetailItem label="Place of work">{formatAddress(project.workplaceAddress)}</DetailItem>

				<DetailItem label="Starts on">{formatDate(project.startsOn)}</DetailItem>

				<DetailItem label="Ends on">
					{project.endsOn ? formatDate(project.endsOn) : "Open-ended"}
				</DetailItem>

				<DetailItem label="Team">{project.teamName ?? "Not assigned"}</DetailItem>

				<DetailItem label="Description">{project.description}</DetailItem>
			</dl>

			<div className="project-section-body">
				<Button variant="ghost" icon={<UsersRound size={15} />} onClick={onAssignTeam}>
					{project.teamId ? "Change team" : "Assign team"}
				</Button>
			</div>
		</div>
	);
}
