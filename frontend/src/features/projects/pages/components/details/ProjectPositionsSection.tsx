import { Link } from "@tanstack/react-router";
import { BriefcaseBusiness, Pencil } from "lucide-react";
import type { PositionListItem, ProjectProjection } from "@/api/models";
import { DetailOverviewHeader, EmptyState } from "@/components/ui";
import { PositionStaffing } from "@/features/positions/components/PositionStaffing";
import { useGetPositionsSlice } from "@/features/positions/pages/hooks";
import { rateUnitShort, workerContractTypes } from "@/features/positions/types";

interface ProjectPositionsSectionProps {
	project: ProjectProjection;
	onOpenPosition?: () => void;
	onEditPosition?: (position: PositionListItem) => void;
}

/**
 * The roles this delivery is staffed with. One client is one project, and inside it painters and
 * bricklayers are two roles with two contracts, two rates and often two addresses — which is the
 * whole reason a position is not a line of free text on somebody's posting.
 *
 * Archived roles are left out here: this is the list somebody picks from, and the register page is
 * where the closed ones are still readable.
 */
export function ProjectPositionsSection({
	project,
	onOpenPosition,
	onEditPosition,
}: ProjectPositionsSectionProps) {
	const query = useGetPositionsSlice({ projectId: project.id });

	const positions = query.data?.pages.flatMap((page) => page.content ?? []) ?? [];

	return (
		<div className="data-overview">
			<DetailOverviewHeader
				title="Positions on this project"
				description="The roles people are posted onto. A role carries the terms their contract asks for, so nobody retypes them per person."
				onAdd={onOpenPosition}
			/>

			{query.isError ? <p className="form-error">The positions could not be loaded.</p> : null}

			{!query.isLoading && positions.length === 0 ? (
				<EmptyState
					title="No roles opened yet"
					description="Nobody can be planned onto this project until it has at least one role."
				>
					<BriefcaseBusiness size={24} />
				</EmptyState>
			) : null}

			{positions.length > 0 ? (
				<>
					<table className="table">
						<thead>
							<tr>
								<th>Position</th>
								<th className="table-header-md">We sign</th>
								<th className="table-header-sm">Rate</th>
								<th className="table-header-sm">Staffed</th>
								<th />
							</tr>
						</thead>

						<tbody>
							{positions.map((position) => (
								<tr key={position.id}>
									<td className="table-cell-truncate" title={position.name}>
										{position.name}
									</td>

									<td>{workerContractTypes[position.contractType]}</td>

									<td className="table-figure">
										{position.proposedRate
											? `${position.proposedRate.amount} ${position.proposedRate.currency}/${
													rateUnitShort[position.proposedRate.unit]
												}`
											: "—"}
									</td>

									<td>
										<PositionStaffing
											assigned={position.assignedCount}
											planned={position.plannedHeadcount}
										/>
									</td>

									<td>
										<div className="flex gap-2 justify-end">
											{onEditPosition ? (
												<button
													type="button"
													className="action-button"
													title="Edit"
													aria-label="Edit"
													onClick={() => onEditPosition(position)}
												>
													<Pencil size={15} />
												</button>
											) : null}
										</div>
									</td>
								</tr>
							))}
						</tbody>
					</table>

					{/* Archived roles and the search across projects live on the register itself. */}
					<Link
						to="/app/positions"
						search={{
							projectId: project.id,
							search: undefined,
							contractType: undefined,
							includeArchived: undefined,
						}}
					>
						See every role on this project, archived ones included
					</Link>
				</>
			) : null}
		</div>
	);
}
