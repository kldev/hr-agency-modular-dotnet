import { Link } from "@tanstack/react-router";
import { Briefcase } from "lucide-react";
import type { WorkerProjection } from "@/api/models";
import { AssignmentStatusBadge, DetailOverviewHeader, EmptyState } from "@/components/ui";
import { engagementTypes } from "@/features/compliance";
import { formatPeriod } from "@/utlis/formatRecord";

interface WorkerAssignmentsSectionProps {
	worker: WorkerProjection;
	onPlanAssignment?: () => void;
}

/**
 * Their history, and it writes itself: moving somebody from one project to another ends one posting
 * and opens another, so the list of postings *is* the employment history. There is deliberately no
 * "move to another project" action — the backend has no command that repoints an assignment, and
 * offering one here would be a lie about the model.
 */
export function WorkerAssignmentsSection({
	worker,
	onPlanAssignment,
}: WorkerAssignmentsSectionProps) {
	const assignments = worker.assignments ?? [];

	return (
		<div className="data-overview">
			<DetailOverviewHeader
				title="Assignments"
				description="Every posting this person has held. A move is a new one, not an edit of the last."
				onAdd={onPlanAssignment}
			/>

			{assignments.length === 0 ? (
				<EmptyState
					title="No postings yet"
					description="Somebody can be planned onto a project at any stage of the pipeline; they just cannot start work until they are through it."
				>
					<Briefcase size={24} />
				</EmptyState>
			) : (
				<table className="table">
					<thead>
						<tr>
							<th>Project</th>
							<th>Client</th>
							<th>Posted by</th>
							<th>Country</th>
							<th>Position</th>
							<th>Period</th>
							<th>Status</th>
						</tr>
					</thead>

					<tbody>
						{assignments.map((assignment) => (
							<tr key={assignment.assignmentId}>
								<td className="table-cell-truncate" title={assignment.projectName}>
									<Link
										to="/app/assignments/$id"
										params={{ id: assignment.assignmentId }}
										search={{ search: undefined }}
									>
										{assignment.projectName}
									</Link>
								</td>

								<td className="table-cell-truncate" title={assignment.clientCompanyName}>
									{assignment.clientCompanyName}
								</td>

								{/* Which of our companies posted them is frozen when the posting is planned:
								    it is the company that has to issue the A1. */}
								<td className="table-cell-truncate" title={assignment.deliveringEntityName}>
									{assignment.deliveringEntityName}
								</td>

								<td>{assignment.workCountry}</td>

								<td className="table-cell-truncate" title={assignment.position}>
									{assignment.position}
									<span className="data-meta"> · {engagementTypes[assignment.engagementType]}</span>
								</td>

								<td className="table-number">
									{formatPeriod(assignment.startsOn, assignment.endsOn)}
								</td>

								<td>
									<AssignmentStatusBadge status={assignment.status} />
								</td>
							</tr>
						))}
					</tbody>
				</table>
			)}
		</div>
	);
}
