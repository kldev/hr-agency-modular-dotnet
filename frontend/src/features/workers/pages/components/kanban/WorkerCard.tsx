import { Link } from "@tanstack/react-router";
import { Globe, MapPin } from "lucide-react";
import type { WorkerProjection } from "#/api/models";
import { Kanban } from "@/components/kanban";
import { getCountryLabel } from "@/components/labels";
import { ItemMark } from "@/components/ui";
import { WorkerActions } from "../table/WorkerActions";
import { WorkerExpiryChip } from "../WorkerExpiryChip";

interface WorkerCardProps {
	worker: WorkerProjection;
	onEdit: (worker: WorkerProjection) => void;
	onChangeStatus: (worker: WorkerProjection) => void;
	onPlanAssignment: (worker: WorkerProjection) => void;
}

export function WorkerCard({ worker, onEdit, onChangeStatus, onPlanAssignment }: WorkerCardProps) {
	const status = worker.status ?? "Recruitment";

	return (
		<Kanban.Card dragId={worker.id ?? ""} column={status} data={worker}>
			<div className="kanban-card-header">
				<ItemMark name={worker.fullName ?? ""} />

				<div className="kanban-card-identity">
					<Link
						to="/app/workers/$id"
						params={{ id: worker.id ?? "" }}
						search={{ search: undefined, tab: undefined }}
						className="kanban-card-title"
					>
						{worker.fullName}
					</Link>

					<div className="kanban-card-subtitle">
						{getCountryLabel(worker.citizenship ?? "")}
						{worker.requiresLegalisation ? " · needs legalisation" : ""}
					</div>
				</div>

				<WorkerActions
					id={worker.id ?? ""}
					onEdit={() => onEdit(worker)}
					onChangeStatus={() => onChangeStatus(worker)}
					onPlanAssignment={() => onPlanAssignment(worker)}
				/>
			</div>

			<div className="kanban-card-details">
				<div className="kanban-card-detail">
					<MapPin size={13} />
					<span>{worker.currentProjectName ?? "No project"}</span>
				</div>

				<div className="kanban-card-detail">
					<Globe size={13} />
					<span>{worker.currentWorkCountry ?? "—"}</span>
				</div>
			</div>

			<div className="kanban-card-footer">
				<span>
					Postings {Number(worker.openAssignmentCount ?? 0)} / {Number(worker.assignmentCount ?? 0)}
				</span>

				<WorkerExpiryChip
					identityDocumentValidUntil={worker.identityDocumentValidUntil}
					nextAuthorisationExpiryOn={worker.nextAuthorisationExpiryOn}
				/>
			</div>
		</Kanban.Card>
	);
}
