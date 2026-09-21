import { Link } from "@tanstack/react-router";
import type { ProjectProjection } from "#/api/models";
import { DetailItem, ProjectStatusBadge } from "#/components/ui";
import { ComplianceChip } from "@/features/compliance";
import { engagementTypes } from "../../../types";
import { formatPeriod } from "../../../utils";
import { ProjectActions } from "./ProjectActions";

interface ProjectsCardListProps {
	projects: ProjectProjection[];
	onEdit: (project: ProjectProjection) => void;
	onChangeStatus: (project: ProjectProjection) => void;
}

export function ProjectsCardList({ projects, onEdit, onChangeStatus }: ProjectsCardListProps) {
	return (
		<div className="data-mobile-view">
			{projects.map<React.ReactNode>((project) => (
				<div
					key={project.id}
					className="flex flex-col gap-3 pb-5 border mb-5 mt-5 shadow-subtle card-border"
				>
					<div className="data-detail-item flex flex-row">
						<div className="grow">
							<dt>Project</dt>
							<dd>
								<Link
									to="/app/projects/$id"
									params={{ id: project.id }}
									search={{ search: undefined, tab: undefined }}
								>
									{project.name}
								</Link>
							</dd>
						</div>

						<ProjectActions
							id={project.id}
							onEdit={() => onEdit(project)}
							onChangeStatus={() => onChangeStatus(project)}
						/>
					</div>

					<dl className="data-details-list">
						<DetailItem label="Client">{project.companyName}</DetailItem>

						<DetailItem label="Status">
							<ProjectStatusBadge status={project.status} />
						</DetailItem>

						<DetailItem label="Engagement">{engagementTypes[project.engagementType]}</DetailItem>

						<DetailItem label="Country">{project.workCountry}</DetailItem>

						<DetailItem label="Period">{formatPeriod(project.startsOn, project.endsOn)}</DetailItem>

						<DetailItem label="Responsible">
							{project.responsibleContact?.fullname ?? "—"}
						</DetailItem>

						<DetailItem label="Compliance">
							<ComplianceChip
								outstanding={project.complianceOutstandingCount}
								nextExpiryOn={project.nextComplianceExpiryOn}
							/>
						</DetailItem>
					</dl>
				</div>
			))}
		</div>
	);
}
