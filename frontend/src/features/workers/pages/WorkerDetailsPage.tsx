import { useParams } from "@tanstack/react-router";
import type React from "react";
import { useRef } from "react";
import type { UserSnapshot } from "@/api/models";
import { getCountryLabel } from "@/components/labels";
import {
	AuditInformation,
	DetailsHeader,
	DetailsLoading,
	WorkerStatusBadge,
} from "@/components/ui";
import { DataDetails, DataDetailsLayout } from "@/components/ui/details/DataDetails";
import {
	AttachWorkerDocumentDrawer,
	type AttachWorkerDocumentFormCommand,
	ChangeWorkerStatusDrawer,
	type ChangeWorkerStatusFormCommand,
	RecordWorkAuthorisationDrawer,
	type RecordWorkAuthorisationFormCommand,
} from "../drawers";
import { type WorkerWizardCommand, WorkerWizardDialog } from "../wizards/worker/WorkerWizardDialog";
import {
	WorkerAssignmentsSection,
	WorkerAuthorisationsSection,
	WorkerContactSection,
	WorkerDocumentsSection,
	WorkerIdentitySection,
	WorkerPipelineSidebar,
} from "./components/details";
import { WorkerActions } from "./components/table/WorkerActions";
import { useGetWorker } from "./hooks";

const WorkerDetailsPage: React.FC = () => {
	const { id } = useParams({ from: "/app/workers/$id" });

	const wizardRef = useRef<WorkerWizardCommand>(null);
	const statusRef = useRef<ChangeWorkerStatusFormCommand>(null);
	const authorisationRef = useRef<RecordWorkAuthorisationFormCommand>(null);
	const documentRef = useRef<AttachWorkerDocumentFormCommand>(null);

	const query = useGetWorker(id);
	const worker = query.data;

	const refresh = () => void query.refetch();

	if (!id || query.isLoading || query.isError || !worker) {
		return (
			<DetailsLoading id={id} isLoading={query.isLoading} isError={query.isError || !worker} />
		);
	}

	return (
		<>
			<DataDetails>
				<DetailsHeader
					name={worker.fullName ?? ""}
					onEdit={() => wizardRef.current?.edit(worker)}
					detailsAddons={
						<>
							<WorkerStatusBadge status={worker.status ?? "Recruitment"} />

							<span className="badge badge-inactive">
								{getCountryLabel(worker.citizenship ?? "")}
							</span>
						</>
					}
					extraAdd={
						<WorkerActions
							id={worker.id ?? ""}
							mode="details"
							onChangeStatus={() => statusRef.current?.changeStatus(worker)}
						/>
					}
				/>

				<DataDetailsLayout
					main={
						<>
							<section className="data-details-section">
								<WorkerIdentitySection worker={worker} />
							</section>

							<section className="data-details-section">
								<WorkerContactSection worker={worker} />
							</section>

							<section className="data-details-section">
								<WorkerDocumentsSection
									worker={worker}
									onAttach={() => documentRef.current?.attach(worker.id ?? "")}
									onRefresh={refresh}
								/>
							</section>

							{/* Only for somebody the legalisation rules apply to - see the section itself. */}
							{worker.requiresLegalisation ? (
								<section className="data-details-section">
									<WorkerAuthorisationsSection
										worker={worker}
										onRecord={() => authorisationRef.current?.record(worker)}
										onRefresh={refresh}
									/>
								</section>
							) : null}

							<section className="data-details-section">
								<WorkerAssignmentsSection worker={worker} />
							</section>
						</>
					}
					sidebar={
						<>
							<section className="data-details-section">
								<WorkerPipelineSidebar worker={worker} />
							</section>

							<AuditInformation
								createdAt={worker.createdAt ?? ""}
								createdBy={worker.createdBy as UserSnapshot}
								modifiedAt={worker.modifiedAt ?? null}
								modifiedBy={worker.modifiedBy ?? null}
							/>
						</>
					}
				/>
			</DataDetails>

			<WorkerWizardDialog ref={wizardRef} onSuccess={refresh} />
			<ChangeWorkerStatusDrawer ref={statusRef} onSuccess={refresh} />
			<RecordWorkAuthorisationDrawer ref={authorisationRef} onSuccess={refresh} />
			<AttachWorkerDocumentDrawer ref={documentRef} onSuccess={refresh} />
		</>
	);
};

export default WorkerDetailsPage;
