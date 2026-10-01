import { Table } from "@heroui/react";
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
				<Table variant="secondary" className="table-container">
					<Table.ScrollContainer>
						<Table.Content aria-label="Assignments">
							<Table.Header>
								<Table.Column isRowHeader>Project</Table.Column>
								<Table.Column>Client</Table.Column>
								<Table.Column>Posted by</Table.Column>
								<Table.Column className="table-header-ssm">Country</Table.Column>
								<Table.Column>Position</Table.Column>
								<Table.Column className="table-header-md">Period</Table.Column>
								<Table.Column className="table-header-sm">Status</Table.Column>
							</Table.Header>

							<Table.Body>
								{assignments.map((assignment) => (
									<Table.Row key={assignment.assignmentId}>
										<Table.Cell className="table-cell-truncate">
											<span title={assignment.projectName}>
												<Link
													to="/app/assignments/$id"
													params={{ id: assignment.assignmentId }}
													search={{ search: undefined }}
												>
													{assignment.projectName}
												</Link>
											</span>
										</Table.Cell>

										<Table.Cell className="table-cell-truncate">
											<span title={assignment.clientCompanyName}>
												{assignment.clientCompanyName}
											</span>
										</Table.Cell>

										{/* Which of our companies posted them is frozen when the posting is planned:
								    it is the company that has to issue the A1. */}
										<Table.Cell className="table-cell-truncate">
											<span title={assignment.deliveringEntityName}>
												{assignment.deliveringEntityName}
											</span>
										</Table.Cell>

										<Table.Cell>{assignment.workCountry}</Table.Cell>

										<Table.Cell className="table-cell-truncate">
											<span title={assignment.positionName}>
												{assignment.positionName}
												<span className="data-meta">
													{" "}
													· {engagementTypes[assignment.engagementType]}
												</span>
											</span>
										</Table.Cell>

										<Table.Cell className="table-figure">
											{formatPeriod(assignment.startsOn, assignment.endsOn)}
										</Table.Cell>

										<Table.Cell>
											<AssignmentStatusBadge status={assignment.status} />
										</Table.Cell>
									</Table.Row>
								))}
							</Table.Body>
						</Table.Content>
					</Table.ScrollContainer>
				</Table>
			)}
		</div>
	);
}
