import { Link } from "@tanstack/react-router";
import { useRef } from "react";
import type { ProjectProjection } from "#/api/models";
import { DetailItem, ProjectStatusBadge } from "#/components/ui";
import { formatDate } from "#/utlis/dateUtils";
import {
	ChangeProjectStatusDrawer,
	type ChangeProjectStatusFormCommand,
	EditProjectDrawer,
	type EditProjectFormCommand,
} from "../../../drawers";
import { engagementTypes } from "../../../types";
import { ComplianceChip } from "../ComplianceChip";
import { ProjectActions } from "./ProjectActions";

interface ProjectsCardListProps {
	projects: ProjectProjection[];
	onRefresh: () => void;
}

export function ProjectsCardList({ projects, onRefresh }: ProjectsCardListProps) {
	const editRef = useRef<EditProjectFormCommand>(null);
	const statusRef = useRef<ChangeProjectStatusFormCommand>(null);

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
									search={{ search: undefined }}
								>
									{project.name}
								</Link>
							</dd>
						</div>

						<ProjectActions
							id={project.id}
							onEdit={() => {
								editRef.current?.edit(project);
							}}
							onChangeStatus={() => {
								statusRef.current?.changeStatus(project);
							}}
						/>
					</div>

					<dl className="data-details-list">
						<DetailItem label="Client">{project.companyName}</DetailItem>

						<DetailItem label="Status">
							<ProjectStatusBadge status={project.status} />
						</DetailItem>

						<DetailItem label="Engagement">{engagementTypes[project.engagementType]}</DetailItem>

						<DetailItem label="Country">{project.workCountry}</DetailItem>

						<DetailItem label="Period">
							{formatDate(project.startsOn)} – {project.endsOn ? formatDate(project.endsOn) : "—"}
						</DetailItem>

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

			<EditProjectDrawer ref={editRef} onSuccess={onRefresh} />
			<ChangeProjectStatusDrawer ref={statusRef} onSuccess={onRefresh} />
		</div>
	);
}
