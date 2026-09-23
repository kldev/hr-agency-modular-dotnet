import type { WorkerProjection, WorkerStatus } from "#/api/models";
import { Kanban, type KanbanMove } from "@/components/kanban";
import { allowedWorkerStatusTransitions } from "../../../types";
import type { WorkersFilters } from "../../hooks";
import { workerPipeline } from "./statusIcons";
import { WorkerCard } from "./WorkerCard";
import { WorkersKanbanColumn } from "./WorkersKanbanColumn";

interface WorkersKanbanProps {
	/** Everything but the status, which each column adds for itself. */
	filters: Omit<WorkersFilters, "status">;
	onEdit: (worker: WorkerProjection) => void;
	onChangeStatus: (worker: WorkerProjection, target?: WorkerStatus) => void;
	onPlanAssignment: (worker: WorkerProjection) => void;
}

/*
 * The pipeline as a board. A drop does not change anything by itself: it opens the status drawer
 * with the column preselected, because a change carries a reason and may be refused for a lapsed
 * document - both belong to the drawer. Columns the person cannot reach are the same mirror of
 * `WorkerStatusChangePolicy` the drawer offers its options from.
 */
export function WorkersKanban({
	filters,
	onEdit,
	onChangeStatus,
	onPlanAssignment,
}: WorkersKanbanProps) {
	const canMove = ({ data, to }: KanbanMove<WorkerProjection>) =>
		allowedWorkerStatusTransitions(
			data.status ?? "Recruitment",
			Boolean(data.requiresLegalisation),
		).some((status) => status === to);

	const onMove = ({ data, to }: KanbanMove<WorkerProjection>) =>
		onChangeStatus(data, to as WorkerStatus);

	const renderCard = (worker: WorkerProjection) => (
		<WorkerCard
			key={worker.id}
			worker={worker}
			onEdit={onEdit}
			onChangeStatus={onChangeStatus}
			onPlanAssignment={onPlanAssignment}
		/>
	);

	return (
		<Kanban<WorkerProjection> label="Workers pipeline" onMove={onMove} canMove={canMove}>
			{workerPipeline.map((status) => (
				<WorkersKanbanColumn
					key={status}
					status={status}
					filters={filters}
					renderCard={renderCard}
				/>
			))}
		</Kanban>
	);
}
