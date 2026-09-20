import type { ProjectProjection } from "@/api/models";
import { DetailItem, DetailOverviewHeader } from "@/components/ui";
import { formatDate } from "@/utlis/dateUtils";
import { engagementTypes } from "../../../types";

interface ProjectOverviewSectionProps {
	project: ProjectProjection;
}

function formatAddress(project: ProjectProjection) {
	const address = project.workplaceAddress;
	const building = address.unitNumber
		? `${address.buildingNumber}/${address.unitNumber}`
		: address.buildingNumber;

	return `${address.street} ${building}, ${address.postalCode} ${address.city}, ${address.countryCode}`;
}

export function ProjectOverviewSection({ project }: ProjectOverviewSectionProps) {
	return (
		<div className="data-overview">
			<DetailOverviewHeader
				title="Overview"
				description="What the engagement is, where it runs and for how long."
			/>

			<dl className="data-details-list">
				<DetailItem label="Engagement type">{engagementTypes[project.engagementType]}</DetailItem>

				<DetailItem label="Place of work">{formatAddress(project)}</DetailItem>

				<DetailItem label="Starts on">{formatDate(project.startsOn)}</DetailItem>

				<DetailItem label="Ends on">
					{project.endsOn ? formatDate(project.endsOn) : "Open-ended"}
				</DetailItem>

				<DetailItem label="Team">{project.teamName ?? "Not assigned"}</DetailItem>

				<DetailItem label="Description">{project.description}</DetailItem>
			</dl>
		</div>
	);
}
