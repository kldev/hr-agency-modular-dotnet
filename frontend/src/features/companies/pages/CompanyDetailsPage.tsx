import { useRef } from "react";

import "./components/details/company-details.css";

import { useParams } from "@tanstack/react-router";
import type { CreateOpportunityRef } from "#/features/sales/components";
import type { CompanyContact } from "@/api/models";
import { ActionButton, DetailsHeader, DetailsLoading } from "@/components/ui";
import { DataDetails, DataDetailsLayout } from "@/components/ui/details/DataDetails";
import {
	type CompleteCompanyProfileCommand,
	CompleteCompanyProfileWizardDialog,
} from "../wizards/complete-profile/CompleteCompanyProfileWizardDialog";
import {
	CompanyContacts,
	CompanyOverview,
	EditCompanyDrawer,
	type EditCompanyFormCommand,
} from "./components";
import { CompanyActions } from "./components/table/CompanyActions";
import { useGetCompany, useGetCompanyContacts } from "./hooks";

export function CompanyDetailsPage() {
	const { id } = useParams({ from: "/app/companies/$id" });
	const editRef = useRef<EditCompanyFormCommand>(null);
	const oppRef = useRef<CreateOpportunityRef>(null);
	const profileRef = useRef<CompleteCompanyProfileCommand>(null);

	const query = useGetCompany(id);
	const contactsQuery = useGetCompanyContacts(id);

	if (!id || query.isLoading || query.isError || !query.data) {
		return (
			<DetailsLoading id={id} isLoading={query.isLoading} isError={query.isError || !query.data} />
		);
	}

	const company = query.data;

	const contacts: CompanyContact[] = contactsQuery.data ?? [];

	return (
		<>
			<DataDetails>
				<DetailsHeader
					name={company.name}
					website={company.website}
					onEdit={() => editRef.current?.edit(company.id)}
					extraAdd={
						company.isProfileComplete ? null : (
							<ActionButton
								title="Complete client data"
								onClick={() => profileRef.current?.complete(company.id)}
							>
								Complete client data
							</ActionButton>
						)
					}
					detailsAddons={
						<CompanyActions
							mode="details"
							id={company.id}
							onAddContact={() => {}}
							onEdit={() => {}}
							onAddOpportunity={() => {
								oppRef.current?.create({ companyId: company.id, companyName: company.name });
							}}
						/>
					}
				/>

				<DataDetailsLayout
					main={
						<section className="data-details-section">
							<CompanyOverview company={company} />
						</section>
					}
					sidebar={
						<section className="data-details-section">
							<CompanyContacts
								companyId={company.id}
								contacts={contacts}
								loading={contactsQuery.isLoading}
								error={contactsQuery.isError}
								onRefresh={async () => {
									await contactsQuery.refetch();
								}}
							/>
						</section>
					}
				/>
				<div className="company-details-grid"></div>
			</DataDetails>

			<EditCompanyDrawer
				ref={editRef}
				onSuccess={() => {
					void query.refetch();
				}}
			/>

			<CompleteCompanyProfileWizardDialog
				ref={profileRef}
				onSuccess={() => {
					void query.refetch();
				}}
			/>
		</>
	);
}
