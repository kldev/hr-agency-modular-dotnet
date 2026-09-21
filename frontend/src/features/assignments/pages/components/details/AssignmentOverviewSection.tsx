import { Link } from "@tanstack/react-router";
import type { AssignmentProjection } from "@/api/models";
import { getCountryLabel } from "@/components/labels";
import { DetailItem, DetailOverviewHeader } from "@/components/ui";
import { engagementTypes } from "@/features/compliance";
import { formatPeriod } from "@/utlis/formatRecord";

interface AssignmentOverviewSectionProps {
	assignment: AssignmentProjection;
}

/**
 * Everything above the line is frozen when the posting is planned and there is no command to change
 * it: `UpdateAssignment` carries the position and the period and nothing else. Moving somebody to
 * another project ends this posting and opens a new one, which is what makes their history a
 * history instead of an overwritten field.
 */
export function AssignmentOverviewSection({ assignment }: AssignmentOverviewSectionProps) {
	return (
		<div className="data-overview">
			<DetailOverviewHeader
				title="Overview"
				description="The person, the project and the company that posted them are fixed. Only the position and the period can still change."
			/>

			<dl className="data-details-list">
				<DetailItem label="Worker">
					<Link
						to="/app/workers/$id"
						params={{ id: assignment.workerId }}
						search={{ search: undefined, tab: undefined }}
					>
						{assignment.workerFullName}
					</Link>
				</DetailItem>

				<DetailItem label="Project">
					<Link
						to="/app/projects/$id"
						params={{ id: assignment.projectId }}
						search={{ search: undefined, tab: undefined }}
					>
						{assignment.projectName}
					</Link>
				</DetailItem>

				<DetailItem label="Client">{assignment.clientCompanyName}</DetailItem>

				<DetailItem label="Posted by">{assignment.deliveringEntityName}</DetailItem>

				<DetailItem label="Work country">{getCountryLabel(assignment.workCountry)}</DetailItem>

				<DetailItem label="Engagement">{engagementTypes[assignment.engagementType]}</DetailItem>

				<DetailItem label="Position">{assignment.positionName}</DetailItem>

				<DetailItem label="Period">
					{formatPeriod(assignment.startsOn, assignment.endsOn)}
				</DetailItem>
			</dl>
		</div>
	);
}
