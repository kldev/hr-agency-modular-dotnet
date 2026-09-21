import { Link } from "@tanstack/react-router";
import type React from "react";
import type { WorkerProjection } from "#/api/models";
import { DetailItem, WorkerStatusBadge } from "#/components/ui";
import { getCountryLabel } from "@/components/labels";
import { WorkerExpiryChip } from "../WorkerExpiryChip";
import { WorkerActions } from "./WorkerActions";

interface WorkersCardListProps {
	workers: WorkerProjection[];
	onEdit?: (worker: WorkerProjection) => void;
	onChangeStatus?: (worker: WorkerProjection) => void;
	onPlanAssignment?: (worker: WorkerProjection) => void;
}

export function WorkersCardList({
	workers,
	onEdit,
	onChangeStatus,
	onPlanAssignment,
}: WorkersCardListProps) {
	return (
		<div className="data-mobile-view">
			{workers.map<React.ReactNode>((worker) => (
				<div
					key={worker.id}
					className="flex flex-col gap-3 pb-5 border mb-5 mt-5 shadow-subtle card-border"
				>
					<div className="data-detail-item flex flex-row">
						<div className="grow">
							<dt>Worker</dt>
							<dd>
								<Link
									to="/app/workers/$id"
									params={{ id: worker.id ?? "" }}
									search={{ search: undefined, tab: undefined }}
								>
									{worker.fullName}
								</Link>
							</dd>
						</div>

						<WorkerActions
							id={worker.id ?? ""}
							onEdit={onEdit && (() => onEdit(worker))}
							onChangeStatus={onChangeStatus && (() => onChangeStatus(worker))}
							onPlanAssignment={onPlanAssignment && (() => onPlanAssignment(worker))}
						/>
					</div>

					<dl className="data-details-list">
						<DetailItem label="Citizenship">
							{getCountryLabel(worker.citizenship ?? "")}
							{worker.requiresLegalisation ? " · needs legalisation" : ""}
						</DetailItem>

						<DetailItem label="Status">
							<WorkerStatusBadge status={worker.status ?? "Recruitment"} />
						</DetailItem>

						<DetailItem label="Works in">{worker.currentWorkCountry ?? "—"}</DetailItem>

						<DetailItem label="Project">{worker.currentProjectName ?? "—"}</DetailItem>

						<DetailItem label="Postings">
							{Number(worker.openAssignmentCount ?? 0)} / {Number(worker.assignmentCount ?? 0)}
						</DetailItem>

						<DetailItem label="Expiry">
							<WorkerExpiryChip
								identityDocumentValidUntil={worker.identityDocumentValidUntil}
								nextAuthorisationExpiryOn={worker.nextAuthorisationExpiryOn}
							/>
						</DetailItem>
					</dl>
				</div>
			))}
		</div>
	);
}
