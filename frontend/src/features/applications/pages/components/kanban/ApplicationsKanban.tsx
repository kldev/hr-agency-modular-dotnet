import { useNavigate } from "@tanstack/react-router";
import { useRef } from "react";
import type { JobApplicationProjection, JobApplicationUpdateStatus } from "#/api/models";
import {
	type WorkerWizardCommand,
	WorkerWizardDialog,
} from "#/features/workers/wizards/worker/WorkerWizardDialog";
import { Kanban, type KanbanMove } from "@/components/kanban";
import type { ApplicationFilters } from "../../../searchParams";
import { allowedApplicationStatusTransitions } from "../../../types";
import {
	ApplicationsActionDrawers,
	type JobApplicationsActionsType,
	type JobApplicationsRef,
} from "../forms";
import { ApplicationCard } from "./ApplicationCard";
import { ApplicationsKanbanColumn } from "./ApplicationsKanbanColumn";
import { applicationPipeline } from "./statusIcons";

interface ApplicationsKanbanProps {
	/** Everything but the status, which each column adds for itself. */
	filters: Omit<ApplicationFilters, "status">;
	onSuccess: () => void;
}

/*
 * The funnel as a board. A drop opens a drawer, it never changes the status by itself: a change
 * carries a note, and a move to `Interview` is scheduling one - the status follows the interview,
 * so that column opens the scheduling drawer instead.
 */
export function ApplicationsKanban({ filters, onSuccess }: ApplicationsKanbanProps) {
	const formRef = useRef<JobApplicationsRef>(null);
	const workerRef = useRef<WorkerWizardCommand>(null);
	const navigate = useNavigate();

	const person = (item: JobApplicationProjection) => ({
		email: item.applicantEmail,
		fullName: item.applicantFullName,
	});

	const onAction = (action: JobApplicationsActionsType, item: JobApplicationProjection) => {
		if (action === "register-worker") {
			workerRef.current?.register({
				applicationId: item.id,
				candidateId: item.candidateId,
				firstName: item.applicantFirstName,
				lastName: item.applicantLastName,
				email: item.applicantEmail,
				phoneNumber: item.applicantPhone,
			});
			return;
		}

		formRef.current?.update(item.id, action, item.status, person(item));
	};

	const canMove = ({ data, to }: KanbanMove<JobApplicationProjection>) =>
		allowedApplicationStatusTransitions(data.status).some((status) => status === to);

	const onMove = ({ data, to }: KanbanMove<JobApplicationProjection>) => {
		if (to === "Interview") {
			formRef.current?.update(data.id, "schedule", data.status, person(data));
			return;
		}

		formRef.current?.changeStatus(data.id, data.status, to as JobApplicationUpdateStatus);
	};

	const renderCard = (application: JobApplicationProjection) => (
		<ApplicationCard
			key={application.id}
			application={application}
			onAction={(action) => onAction(action, application)}
		/>
	);

	return (
		<>
			<Kanban<JobApplicationProjection>
				label="Applications pipeline"
				onMove={onMove}
				canMove={canMove}
			>
				{applicationPipeline.map((status) => (
					<ApplicationsKanbanColumn
						key={status}
						status={status}
						filters={filters}
						renderCard={renderCard}
					/>
				))}
			</Kanban>

			<ApplicationsActionDrawers ref={formRef} onSuccess={onSuccess} />

			<WorkerWizardDialog
				ref={workerRef}
				onSuccess={(workerId) => {
					navigate({
						to: "/app/workers/$id",
						params: { id: workerId },
						search: { search: undefined, tab: undefined },
					});
				}}
			/>
		</>
	);
}
