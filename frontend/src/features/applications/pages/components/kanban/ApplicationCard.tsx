import { Link } from "@tanstack/react-router";
import { Building2, Mail } from "lucide-react";
import type { JobApplicationProjection } from "#/api/models";
import { formatDateTime } from "#/utlis";
import { Kanban } from "@/components/kanban";
import { CandidateSourceBadge, ItemMark } from "@/components/ui";
import { WorkerFileLink } from "@/features/workers/components/WorkerFileLink";
import type { JobApplicationsActionsType } from "../forms";
import { ApplicationsActions } from "../table/ApplicationsActions";

interface ApplicationCardProps {
	application: JobApplicationProjection;
	onAction: (action: JobApplicationsActionsType) => void;
}

export function ApplicationCard({ application, onAction }: ApplicationCardProps) {
	return (
		<Kanban.Card dragId={application.id} column={application.status} data={application}>
			<div className="kanban-card-header">
				<ItemMark name={application.applicantFullName} />

				<div className="kanban-card-identity">
					<Link
						to="/app/applications/$id"
						params={{ id: application.id }}
						search={{ status: undefined, search: undefined, source: undefined, tab: undefined }}
						className="kanban-card-title"
					>
						{application.applicantFullName}
					</Link>

					<div className="kanban-card-subtitle">{application.jobPostTitle}</div>
				</div>

				<ApplicationsActions
					id={application.id}
					workerId={application.workerId}
					onAction={onAction}
				/>
			</div>

			<div className="kanban-card-details">
				<div className="kanban-card-detail">
					<Building2 size={13} />
					<span>{application.company.name}</span>
				</div>

				<div className="kanban-card-detail">
					<Mail size={13} />
					<span>{application.applicantEmail}</span>
				</div>

				<div className="kanban-card-detail">
					<CandidateSourceBadge source={application.source} />
					<WorkerFileLink workerId={application.workerId} />
				</div>
			</div>

			<div className="kanban-card-footer">
				<span>Applied</span>
				<span>{formatDateTime(application.createdAt)}</span>
			</div>
		</Kanban.Card>
	);
}
