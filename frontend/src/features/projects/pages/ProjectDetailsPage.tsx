import { useParams } from "@tanstack/react-router";
import { useRef } from "react";
import type { ContactRole, EmailPurpose } from "@/api/models";
import {
	AuditInformation,
	DetailsHeader,
	DetailsLoading,
	ProjectStatusBadge,
	type TabDefinition,
	TabPanel,
	Tabs,
} from "@/components/ui";
import { DataDetails, DataDetailsLayout } from "@/components/ui/details/DataDetails";
import {
	AssignProjectContactDrawer,
	type AssignProjectContactFormCommand,
	AssignProjectTeamDrawer,
	type AssignProjectTeamFormCommand,
	AttachDocumentDrawer,
	type AttachDocumentFormCommand,
	type ChangeProjectLegalEntityCommand,
	ChangeProjectLegalEntityDrawer,
	ChangeProjectStatusDrawer,
	type ChangeProjectStatusFormCommand,
	RecordComplianceDrawer,
	type RecordComplianceFormCommand,
	RecordContractDrawer,
	type RecordContractFormCommand,
	SetProjectEmailsDrawer,
	type SetProjectEmailsFormCommand,
} from "../drawers";
import { engagementTypes } from "../types";

import "./components/details/project-details.css";

import {
	type PlanAssignmentWizardCommand,
	PlanAssignmentWizardDialog,
} from "#/features/assignments/wizards/plan/PlanAssignmentWizardDialog";
import {
	type CompleteCompanyProfileCommand,
	CompleteCompanyProfileWizardDialog,
} from "#/features/companies/wizards/complete-profile/CompleteCompanyProfileWizardDialog";
import {
	type PositionWizardCommand,
	PositionWizardDialog,
} from "#/features/positions/wizards/position/PositionWizardDialog";
import {
	type ProjectWizardCommand,
	ProjectWizardDialog,
} from "../wizards/project/ProjectWizardDialog";
import {
	ProjectAssignmentsSection,
	ProjectComplianceSection,
	ProjectContactsSection,
	ProjectContractSection,
	ProjectCustomerSection,
	ProjectDocumentsSection,
	ProjectOverviewSection,
	ProjectPositionsSection,
	ProjectStatusSidebar,
} from "./components/details";
import { ProjectActions } from "./components/table/ProjectActions";
import { useGetProject, useRemoveProjectContact } from "./hooks";

export type ProjectTab =
	| "overview"
	| "contract"
	| "positions"
	| "people"
	| "documents"
	| "compliance";

export const projectTabs: readonly ProjectTab[] = [
	"overview",
	"contract",
	"positions",
	"people",
	"documents",
	"compliance",
];

interface ProjectDetailsPageProps {
	tab: ProjectTab;
	onTabChange: (tab: ProjectTab) => void;
}

/**
 * The same layout the worker register settled on: a live project carries a client profile, a
 * contract, its people, its documents and a compliance list, and stacked they are one long scroll
 * where the sections that need attention sit furthest down. The open section is in the URL, so a
 * link can point at a project's compliance rather than at the project.
 */
export function ProjectDetailsPage({ tab, onTabChange }: ProjectDetailsPageProps) {
	const { id } = useParams({ from: "/app/projects/$id" });

	const editRef = useRef<ProjectWizardCommand>(null);
	const statusRef = useRef<ChangeProjectStatusFormCommand>(null);
	const legalEntityRef = useRef<ChangeProjectLegalEntityCommand>(null);
	const contactRef = useRef<AssignProjectContactFormCommand>(null);
	const emailsRef = useRef<SetProjectEmailsFormCommand>(null);
	const contractRef = useRef<RecordContractFormCommand>(null);
	const documentRef = useRef<AttachDocumentFormCommand>(null);
	const complianceRef = useRef<RecordComplianceFormCommand>(null);
	const teamRef = useRef<AssignProjectTeamFormCommand>(null);
	const profileRef = useRef<CompleteCompanyProfileCommand>(null);
	const planAssignmentRef = useRef<PlanAssignmentWizardCommand>(null);
	const positionRef = useRef<PositionWizardCommand>(null);

	const query = useGetProject(id);

	const { mutation: removeContact } = useRemoveProjectContact({});

	if (!id || query.isLoading || query.isError || !query.data) {
		return (
			<DetailsLoading id={id} isLoading={query.isLoading} isError={query.isError || !query.data} />
		);
	}

	const project = query.data;

	const refresh = () => {
		void query.refetch();
	};

	const tabs: TabDefinition<ProjectTab>[] = [
		{ id: "overview", label: "Overview" },
		{ id: "contract", label: "Contract & contacts", count: project.contacts.length },
		/* The roles this delivery is staffed with; the people on them are the next tab along. */
		{ id: "positions", label: "Positions", count: Number(project.openPositionCount ?? 0) },
		{ id: "people", label: "People" },
		{ id: "documents", label: "Documents", count: Number(project.documentCount ?? 0) },
		{
			id: "compliance",
			label: "Compliance",
			/* Outstanding rather than total: zero here means there is nothing left to chase. */
			count: Number(project.complianceOutstandingCount ?? 0),
		},
	];

	const active = tabs.some((candidate) => candidate.id === tab) ? tab : "overview";

	return (
		<>
			<DataDetails>
				<DetailsHeader
					name={project.name}
					onEdit={() => editRef.current?.edit(project)}
					detailsAddons={
						<>
							<ProjectStatusBadge status={project.status} />

							<span className="badge badge-inactive">
								{engagementTypes[project.engagementType]}
							</span>
						</>
					}
					extraAdd={
						<ProjectActions
							mode="details"
							id={project.id}
							onEdit={() => editRef.current?.edit(project)}
							onChangeStatus={() => statusRef.current?.changeStatus(project)}
						/>
					}
				/>

				<Tabs value={active} tabs={tabs} onChange={onTabChange} label="Project sections" />

				<DataDetailsLayout
					main={
						<TabPanel id={active}>
							{active === "overview" ? (
								<>
									<section className="data-details-section">
										<ProjectOverviewSection
											project={project}
											onAssignTeam={() => teamRef.current?.assignTeam(project)}
										/>
									</section>

									<section className="data-details-section">
										<ProjectCustomerSection
											project={project}
											onCompleteProfile={(companyId) => profileRef.current?.complete(companyId)}
											onChangeLegalEntity={() => legalEntityRef.current?.change(project)}
										/>
									</section>
								</>
							) : null}

							{/* The contract and the people who signed it: both answer "who agreed to what". */}
							{active === "contract" ? (
								<>
									<section className="data-details-section">
										<ProjectContractSection
											project={project}
											onRecordContract={() => contractRef.current?.record(project)}
											onEditEmails={(purpose: EmailPurpose) =>
												emailsRef.current?.edit(project, purpose)
											}
										/>
									</section>

									<section className="data-details-section">
										<ProjectContactsSection
											project={project}
											onAssign={(role: ContactRole) => contactRef.current?.assign(project, role)}
											onRemove={(role: ContactRole) => {
												removeContact.mutate({ projectId: project.id, role });
											}}
										/>
									</section>
								</>
							) : null}

							{active === "positions" ? (
								<section className="data-details-section">
									<ProjectPositionsSection
										project={project}
										onOpenPosition={() => positionRef.current?.open(project.id)}
										onEditPosition={(position) => positionRef.current?.edit(position.id)}
									/>
								</section>
							) : null}

							{active === "people" ? (
								<section className="data-details-section">
									<ProjectAssignmentsSection
										project={project}
										onPlanAssignment={() =>
											planAssignmentRef.current?.plan({ projectId: project.id })
										}
									/>
								</section>
							) : null}

							{active === "documents" ? (
								<section className="data-details-section">
									<ProjectDocumentsSection
										project={project}
										onAttach={() => documentRef.current?.attach(project.id)}
										onRefresh={refresh}
									/>
								</section>
							) : null}

							{active === "compliance" ? (
								<section className="data-details-section">
									<ProjectComplianceSection
										project={project}
										onRecord={(view) => complianceRef.current?.record(project, view)}
									/>
								</section>
							) : null}
						</TabPanel>
					}
					sidebar={
						<>
							{/* Outside the tabs on purpose: whether the project can go live, and what is
							    still missing for it, is worth seeing whichever section is open. */}
							<section className="data-details-section">
								<ProjectStatusSidebar project={project} />
							</section>

							<AuditInformation
								createdAt={project.createdAt}
								createdBy={project.createdBy}
								modifiedAt={project.modifiedAt}
								modifiedBy={project.modifiedBy}
							/>
						</>
					}
				/>
			</DataDetails>

			<ProjectWizardDialog ref={editRef} onSuccess={refresh} />
			<ChangeProjectLegalEntityDrawer ref={legalEntityRef} onSuccess={refresh} />
			<ChangeProjectStatusDrawer ref={statusRef} onSuccess={refresh} />
			<AssignProjectContactDrawer ref={contactRef} onSuccess={refresh} />
			<SetProjectEmailsDrawer ref={emailsRef} onSuccess={refresh} />
			<RecordContractDrawer ref={contractRef} onSuccess={refresh} />
			<AttachDocumentDrawer ref={documentRef} onSuccess={refresh} />
			<RecordComplianceDrawer ref={complianceRef} onSuccess={refresh} />
			<AssignProjectTeamDrawer ref={teamRef} onSuccess={refresh} />
			<CompleteCompanyProfileWizardDialog ref={profileRef} onSuccess={refresh} />
			<PlanAssignmentWizardDialog ref={planAssignmentRef} onSuccess={refresh} />
			<PositionWizardDialog ref={positionRef} onSuccess={refresh} />
		</>
	);
}
