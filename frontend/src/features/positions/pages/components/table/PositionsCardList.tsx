import { Link } from "@tanstack/react-router";
import type { PositionListItem } from "#/api/models";
import { DetailItem } from "#/components/ui";
import { PositionStaffing } from "../../../components/PositionStaffing";
import { rateUnitShort, workerContractTypes } from "../../../types";
import { PositionActions } from "./PositionActions";

interface PositionsCardListProps {
	positions: PositionListItem[];
	onEdit: (position: PositionListItem) => void;
	onArchive: (position: PositionListItem) => void;
	onRestore: (position: PositionListItem) => void;
}

export function PositionsCardList({
	positions,
	onEdit,
	onArchive,
	onRestore,
}: PositionsCardListProps) {
	return (
		<div className="data-mobile-view">
			{positions.map<React.ReactNode>((position) => (
				<div
					key={position.id}
					className="flex flex-col gap-3 pb-5 border mb-5 mt-5 shadow-subtle card-border"
				>
					<div className="data-detail-item flex flex-row">
						<div className="grow">
							<dt>Position</dt>
							<dd>{position.name}</dd>
						</div>

						<PositionActions
							isArchived={position.isArchived}
							onEdit={() => onEdit(position)}
							onArchive={() => onArchive(position)}
							onRestore={() => onRestore(position)}
						/>
					</div>

					<dl className="data-details-list">
						<DetailItem label="Project">
							<Link
								to="/app/projects/$id"
								params={{ id: position.projectId }}
								search={{ search: undefined, tab: "positions" as const }}
							>
								{position.projectName}
							</Link>
						</DetailItem>

						<DetailItem label="Client">{position.clientCompanyName}</DetailItem>

						<DetailItem label="Country">{position.workCountry}</DetailItem>

						<DetailItem label="We sign">{workerContractTypes[position.contractType]}</DetailItem>

						<DetailItem label="Rate">
							{position.proposedRate
								? `${position.proposedRate.amount} ${position.proposedRate.currency}/${
										rateUnitShort[position.proposedRate.unit]
									}`
								: "—"}
						</DetailItem>

						<DetailItem label="Staffed">
							<PositionStaffing
								assigned={position.assignedCount}
								planned={position.plannedHeadcount}
							/>
						</DetailItem>
					</dl>
				</div>
			))}
		</div>
	);
}
