import type { WorkerProjection } from "@/api/models";
import { DetailItem, DetailOverviewHeader, WorkerStatusBadge } from "@/components/ui";
import { workerStatusDescriptions } from "../../../types";
import { WorkerExpiryChip } from "../WorkerExpiryChip";

interface WorkerPipelineSidebarProps {
	worker: WorkerProjection;
}

/**
 * The pipeline has an owner per stage, which is the useful half of the status: it says whose queue
 * this person is sitting in right now.
 */
export function WorkerPipelineSidebar({ worker }: WorkerPipelineSidebarProps) {
	const status = worker.status ?? "Recruitment";

	return (
		<div className="data-overview">
			<DetailOverviewHeader title="Pipeline" description={workerStatusDescriptions[status]} />

			<dl className="data-details-list">
				<DetailItem label="Status">
					<WorkerStatusBadge status={status} />
				</DetailItem>

				<DetailItem label="Works in">{worker.currentWorkCountry ?? "Not on a project"}</DetailItem>

				<DetailItem label="Current project">{worker.currentProjectName ?? "—"}</DetailItem>

				<DetailItem label="Postings">
					<span className="data-detail-number">
						{Number(worker.openAssignmentCount ?? 0)} open of {Number(worker.assignmentCount ?? 0)}
					</span>
				</DetailItem>

				<DetailItem label="Documents">
					<span className="data-detail-number">{Number(worker.documentCount ?? 0)}</span>
				</DetailItem>

				<DetailItem label="Next expiry">
					<WorkerExpiryChip
						identityDocumentValidUntil={worker.identityDocumentValidUntil}
						nextAuthorisationExpiryOn={worker.nextAuthorisationExpiryOn}
					/>
				</DetailItem>
			</dl>
		</div>
	);
}
