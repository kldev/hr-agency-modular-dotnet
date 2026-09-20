import { useParams } from "@tanstack/react-router";
import { useRef } from "react";
import { toast } from "sonner";
import type { ContactRole, EmailPurpose } from "@/api/models";
import {
	ActionButton,
	AuditInformation,
	DetailsHeader,
	DetailsLoading,
	ProjectStatusBadge,
} from "@/components/ui";
import { DataDetails, DataDetailsLayout } from "@/components/ui/details/DataDetails";
import {
	AssignProjectContactDrawer,
	type AssignProjectContactFormCommand,
	ChangeProjectStatusDrawer,
	type ChangeProjectStatusFormCommand,
	EditProjectDrawer,
	type EditProjectFormCommand,
	RecordContractDrawer,
	type RecordContractFormCommand,
	SetProjectEmailsDrawer,
	type SetProjectEmailsFormCommand,
} from "../drawers";
import { engagementTypes } from "../types";

import "./components/details/project-details.css";

import {
	type CompleteCompanyProfileCommand,
	CompleteCompanyProfileWizardDialog,
} from "#/features/companies/wizards/complete-profile/CompleteCompanyProfileWizardDialog";
import {
	ProjectContactsSection,
	ProjectContractSection,
	ProjectCustomerSection,
	ProjectOverviewSection,
	ProjectStatusSidebar,
} from "./components/details";
import { useGetProject, useRemoveProjectContact } from "./hooks";

export function ProjectDetailsPage() {
	const { id } = useParams({ from: "/app/projects/$id" });

	const editRef = useRef<EditProjectFormCommand>(null);
	const statusRef = useRef<ChangeProjectStatusFormCommand>(null);
	const contactRef = useRef<AssignProjectContactFormCommand>(null);
	const emailsRef = useRef<SetProjectEmailsFormCommand>(null);
	const contractRef = useRef<RecordContractFormCommand>(null);
	const profileRef = useRef<CompleteCompanyProfileCommand>(null);

	const query = useGetProject(id);

	const { mutation: removeContact } = useRemoveProjectContact({
		onSuccess: () => {
			toast.success("Contact removed");
		},
	});

	if (!id || query.isLoading || query.isError || !query.data) {
		return (
			<DetailsLoading id={id} isLoading={query.isLoading} isError={query.isError || !query.data} />
		);
	}

	const project = query.data;

	const refresh = () => {
		void query.refetch();
	};

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
						<ActionButton
							title="Change status"
							onClick={() => statusRef.current?.changeStatus(project)}
						>
							Change status
						</ActionButton>
					}
				/>

				<DataDetailsLayout
					main={
						<>
							<section className="data-details-section">
								<ProjectOverviewSection project={project} />
							</section>

							<section className="data-details-section">
								<ProjectCustomerSection
									project={project}
									onCompleteProfile={(companyId) => profileRef.current?.complete(companyId)}
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

							<section className="data-details-section">
								<ProjectContractSection
									project={project}
									onRecordContract={() => contractRef.current?.record(project)}
									onEditEmails={(purpose: EmailPurpose) =>
										emailsRef.current?.edit(project, purpose)
									}
								/>
							</section>
						</>
					}
					sidebar={
						<>
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

			<EditProjectDrawer ref={editRef} onSuccess={refresh} />
			<ChangeProjectStatusDrawer ref={statusRef} onSuccess={refresh} />
			<AssignProjectContactDrawer ref={contactRef} onSuccess={refresh} />
			<SetProjectEmailsDrawer ref={emailsRef} onSuccess={refresh} />
			<RecordContractDrawer ref={contractRef} onSuccess={refresh} />
			<CompleteCompanyProfileWizardDialog ref={profileRef} onSuccess={refresh} />
		</>
	);
}
