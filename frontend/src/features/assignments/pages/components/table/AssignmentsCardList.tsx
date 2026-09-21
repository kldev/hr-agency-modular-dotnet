import { Link } from "@tanstack/react-router";
import type React from "react";
import type { AssignmentProjection } from "#/api/models";
import { AssignmentStatusBadge, DetailItem } from "#/components/ui";
import { ComplianceChip, engagementTypes } from "@/features/compliance";
import { formatPeriod } from "@/utlis/formatRecord";
import { AssignmentActions } from "./AssignmentActions";

interface AssignmentsCardListProps {
	assignments: AssignmentProjection[];
	onEdit?: (assignment: AssignmentProjection) => void;
	onChangeStatus?: (assignment: AssignmentProjection) => void;
}

export function AssignmentsCardList({
	assignments,
	onEdit,
	onChangeStatus,
}: AssignmentsCardListProps) {
	return (
		<div className="data-mobile-view">
			{assignments.map<React.ReactNode>((assignment) => (
				<div
					key={assignment.id}
					className="flex flex-col gap-3 pb-5 border mb-5 mt-5 shadow-subtle card-border"
				>
					<div className="data-detail-item flex flex-row">
						<div className="grow">
							<dt>Worker</dt>
							<dd>
								<Link
									to="/app/assignments/$id"
									params={{ id: assignment.id }}
									search={{ search: undefined }}
								>
									{assignment.workerFullName}
								</Link>
							</dd>
						</div>

						<AssignmentActions
							id={assignment.id}
							onEdit={onEdit && (() => onEdit(assignment))}
							onChangeStatus={onChangeStatus && (() => onChangeStatus(assignment))}
						/>
					</div>

					<dl className="data-details-list">
						<DetailItem label="Project">{assignment.projectName}</DetailItem>
						<DetailItem label="Client">{assignment.clientCompanyName}</DetailItem>
						<DetailItem label="Posted by">{assignment.deliveringEntityName}</DetailItem>
						<DetailItem label="Country">{assignment.workCountry}</DetailItem>
						<DetailItem label="Position">{assignment.positionName}</DetailItem>

						<DetailItem label="Engagement">{engagementTypes[assignment.engagementType]}</DetailItem>

						<DetailItem label="Period">
							{formatPeriod(assignment.startsOn, assignment.endsOn)}
						</DetailItem>

						<DetailItem label="Status">
							<AssignmentStatusBadge status={assignment.status} />
						</DetailItem>

						<DetailItem label="Compliance">
							<ComplianceChip
								outstanding={assignment.complianceOutstandingCount}
								nextExpiryOn={assignment.nextComplianceExpiryOn}
							/>
						</DetailItem>
					</dl>
				</div>
			))}
		</div>
	);
}
