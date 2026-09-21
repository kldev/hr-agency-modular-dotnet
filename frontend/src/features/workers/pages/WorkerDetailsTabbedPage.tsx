import { Link, useParams } from "@tanstack/react-router";
import type React from "react";
import { useRef } from "react";
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

export type WorkerTab = "overview" | "documents" | "permissions" | "assignments";

export const workerTabs: readonly WorkerTab[] = [
	"overview",
	"documents",
	"permissions",
	"assignments",
];

interface WorkerDetailsTabbedPageProps {
	tab: WorkerTab;
	onTabChange: (tab: WorkerTab) => void;
}

/**
 * The second layout for the same data, kept side by side with the stacked one so the two can be
 * compared on real records rather than argued about.
 *
 * The case for it: somebody with thirty documents and a dozen postings turns the stacked page into
 * a scroll, and the sections that matter are the ones furthest down. Tabs give each of them the
 * full width and a count you can read without scrolling. The case against it: this is the first
 * tabbed screen in the app, so it is a second navigation idiom to learn and to maintain.
 *
 * The selected tab lives in the URL, so a link can point at somebody's documents rather than at
 * their page.
 */
const WorkerDetailsTabbedPage: React.FC<WorkerDetailsTabbedPageProps> = ({ tab, onTabChange }) => {
	const { id } = useParams({ from: "/app/workers/tabs/$id" });

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
						<>
							<Link
								to="/app/workers/$id"
								params={{ id }}
								search={{ search: undefined }}
								className="data-details-website"
							>
								Stacked layout
							</Link>

							<WorkerActions
								id={id}
								mode="details"
								onChangeStatus={() => statusRef.current?.changeStatus(worker)}
							/>
						</>
					}
				/>

				<Tabs value={active} tabs={tabs} onChange={onTabChange} label="Worker sections" />

				<DataDetailsLayout
					main={
						<TabPanel id={active}>
							{active === "overview" ? (
								<>
									<section className="data-details-section">
										<WorkerIdentitySection worker={worker} />
									</section>

									<section className="data-details-section">
										<WorkerContactSection worker={worker} />
									</section>
								</>
							) : null}

							{active === "documents" ? (
								<section className="data-details-section">
									<WorkerDocumentsSection
										worker={worker}
										onAttach={() => documentRef.current?.attach(id)}
										onRefresh={refresh}
									/>
								</section>
							) : null}

							{active === "permissions" ? (
								<section className="data-details-section">
									<WorkerAuthorisationsSection
										worker={worker}
										onRecord={() => authorisationRef.current?.record(worker)}
										onRefresh={refresh}
									/>
								</section>
							) : null}

							{active === "assignments" ? (
								<section className="data-details-section">
									<WorkerAssignmentsSection worker={worker} />
								</section>
							) : null}
						</TabPanel>
					}
					sidebar={
						<>
							{/* Outside the tabs on purpose: whose desk this person is on is the one thing
							    worth seeing whichever section you opened. */}
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

export default WorkerDetailsTabbedPage;
