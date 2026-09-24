import { useParams } from "@tanstack/react-router";
import type React from "react";
import { useRef } from "react";
import {
	type PlanAssignmentWizardCommand,
	PlanAssignmentWizardDialog,
} from "#/features/assignments/wizards/plan/PlanAssignmentWizardDialog";
import { WorkerFormsSection } from "#/features/forms/responses/WorkerFormsSection";
import type { UserSnapshot } from "@/api/models";
import { getCountryLabel } from "@/components/labels";
import {
	AuditInformation,
	DetailsHeader,
	DetailsLoading,
	type TabDefinition,
	TabPanel,
	Tabs,
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

export type WorkerTab = "overview" | "documents" | "permissions" | "assignments" | "forms";

export const workerTabs: readonly WorkerTab[] = [
	"overview",
	"documents",
	"permissions",
	"assignments",
	"forms",
];

interface WorkerDetailsPageProps {
	tab: WorkerTab;
	onTabChange: (tab: WorkerTab) => void;
}

/**
 * Tabs rather than one long page, which is the first tabbed screen in the app and was chosen after
 * running both layouts side by side: somebody with thirty documents and a dozen postings turns a
 * stacked page into a scroll where the sections that matter are the ones furthest down. Each
 * section gets the full width and a count that can be read without scrolling.
 *
 * The selected tab lives in the URL, so a link can point at somebody's documents rather than at
 * their page.
 */
const WorkerDetailsPage: React.FC<WorkerDetailsPageProps> = ({ tab, onTabChange }) => {
	const { id } = useParams({ from: "/app/workers/$id" });

	const wizardRef = useRef<WorkerWizardCommand>(null);
	const statusRef = useRef<ChangeWorkerStatusFormCommand>(null);
	const authorisationRef = useRef<RecordWorkAuthorisationFormCommand>(null);
	const documentRef = useRef<AttachWorkerDocumentFormCommand>(null);
	const planAssignmentRef = useRef<PlanAssignmentWizardCommand>(null);

	const query = useGetWorker(id);
	const worker = query.data;

	const refresh = () => void query.refetch();

	if (!id || query.isLoading || query.isError || !worker) {
		return (
			<DetailsLoading id={id} isLoading={query.isLoading} isError={query.isError || !worker} />
		);
	}

	/* Permissions only exist for somebody the legalisation rules apply to, so neither does the tab. */
	const tabs: TabDefinition<WorkerTab>[] = [
		{ id: "overview", label: "Overview" },
		{ id: "documents", label: "Documents", count: Number(worker.documentCount ?? 0) },
		...(worker.requiresLegalisation
			? [
					{
						id: "permissions" as const,
						label: "Permissions",
						count: worker.authorisations?.length ?? 0,
					},
				]
			: []),
		{
			id: "assignments",
			label: "Assignments",
			count: Number(worker.assignmentCount ?? 0),
		},
		{ id: "forms", label: "Forms" },
	];

	/* A tab that does not apply to this person falls back rather than showing an empty page. */
	const active = tabs.some((candidate) => candidate.id === tab) ? tab : "overview";

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
							id={id}
							mode="details"
							onChangeStatus={() => statusRef.current?.changeStatus(worker)}
							onPlanAssignment={() => planAssignmentRef.current?.plan({ workerId: id })}
						/>
					}
				/>

				<Tabs value={active} tabs={tabs} onChange={onTabChange} label="Worker sections" />

				<DataDetailsLayout
					main={
						<TabPanel id={active}>
							{active === "overview" ? (
								<>
									<section className="data-details-section mt-1">
										<WorkerIdentitySection worker={worker} />
									</section>

									<section className="data-details-section mt-5">
										<WorkerContactSection worker={worker} />
									</section>
								</>
							) : null}

							{active === "documents" ? (
								<section className="data-details-section mt-1">
									<WorkerDocumentsSection
										worker={worker}
										onAttach={() => documentRef.current?.attach(id)}
										onRefresh={refresh}
									/>
								</section>
							) : null}

							{active === "permissions" ? (
								<section className="data-details-section mt-1">
									<WorkerAuthorisationsSection
										worker={worker}
										onRecord={() => authorisationRef.current?.record(worker)}
										onRefresh={refresh}
									/>
								</section>
							) : null}

							{active === "assignments" ? (
								<section className="data-details-section section mt-1">
									<WorkerAssignmentsSection
										worker={worker}
										onPlanAssignment={() => planAssignmentRef.current?.plan({ workerId: id })}
									/>
								</section>
							) : null}

							{active === "forms" ? (
								<section className="data-details-section mt-1">
									<WorkerFormsSection workerId={id} />
								</section>
							) : null}
						</TabPanel>
					}
					sidebar={
						<>
							{/* Outside the tabs on purpose: whose desk this person is on is the one thing
							    worth seeing whichever section you opened. */}
							<section className="data-details-section mt-1">
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
			<PlanAssignmentWizardDialog ref={planAssignmentRef} onSuccess={refresh} />
		</>
	);
};

export default WorkerDetailsPage;
