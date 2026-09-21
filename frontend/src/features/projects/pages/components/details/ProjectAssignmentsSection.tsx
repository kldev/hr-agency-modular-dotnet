import { Link } from "@tanstack/react-router";
import { Users } from "lucide-react";
import type { ProjectProjection } from "@/api/models";
import { AssignmentStatusBadge, DetailOverviewHeader, EmptyState } from "@/components/ui";
import { useGetAssignmentsSlice } from "@/features/assignments/pages/hooks";
import { ComplianceChip, engagementTypes } from "@/features/compliance";
import { formatPeriod } from "@/utlis/formatRecord";

interface ProjectAssignmentsSectionProps {
	project: ProjectProjection;
	onPlanAssignment?: () => void;
}

/**
 * Who of ours actually works here. The project knows the client, the contract and the country; only
 * the assignments know the people, because a posting belongs to the person as much as to the
 * delivery — which is why they are their own aggregate rather than a list on this page.
 */
export function ProjectAssignmentsSection({
	project,
	onPlanAssignment,
}: ProjectAssignmentsSectionProps) {
	const query = useGetAssignmentsSlice({ projectId: project.id });

	const assignments = query.data?.pages.flatMap((page) => page.content ?? []) ?? [];

	return (
		<div className="data-overview">
			<DetailOverviewHeader
				title="People on this project"
				description="Every posting to this project. Moving somebody off it ends theirs and opens another."
				onAdd={onPlanAssignment}
			/>

			{query.isError ? <p className="form-error">The assignments could not be loaded.</p> : null}

			{!query.isLoading && assignments.length === 0 ? (
				<EmptyState
					title="Nobody is posted here yet"
					description="A project can be sold, signed and live before the first person is planned onto it."
				>
					<Users size={24} />
				</EmptyState>
			) : null}

			{assignments.length > 0 ? (
				<>
					<table className="table">
						<thead>
							<tr>
								<th>Worker</th>
								<th>Position</th>
								<th>Engagement</th>
								<th className="table-header-md">Period</th>
								<th className="table-header-sm">Status</th>
								<th className="table-header-sm">Compliance</th>
							</tr>
						</thead>

						<tbody>
							{assignments.map((assignment) => (
								<tr key={assignment.id}>
									<td className="table-cell-truncate" title={assignment.workerFullName}>
										<Link
											to="/app/assignments/$id"
											params={{ id: assignment.id }}
											search={{ search: undefined }}
										>
											{assignment.workerFullName}
										</Link>
									</td>

									<td className="table-cell-truncate" title={assignment.position}>
										{assignment.position}
									</td>

									<td>{engagementTypes[assignment.engagementType]}</td>

									<td className="table-figure">
										{formatPeriod(assignment.startsOn, assignment.endsOn)}
									</td>

									<td>
										<AssignmentStatusBadge status={assignment.status} />
									</td>

									<td>
										<ComplianceChip
											outstanding={assignment.complianceOutstandingCount}
											nextExpiryOn={assignment.nextComplianceExpiryOn}
										/>
									</td>
								</tr>
							))}
						</tbody>
					</table>

					{/* Only the first page is shown here; the register itself does paging and filtering. */}
					{query.hasNextPage ? (
						<Link
							to="/app/assignments"
							search={{ projectId: project.id, search: undefined, status: undefined }}
						>
							See all postings to this project
						</Link>
					) : null}
				</>
			) : null}
		</div>
	);
}
