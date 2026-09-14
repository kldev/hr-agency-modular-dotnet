import { useRef } from "react";

import "./components/details/company-details.css";

import { useParams } from "@tanstack/react-router";
import type { CompanyContact } from "@/api/models";
import { DetailsHeader } from "@/components/ui";
import { DataDetails, DataDetailsLayout } from "@/components/ui/details/DataDetails";
import {
	CompanyContacts,
	CompanyOverview,
	EditCompanyDrawer,
	type EditCompanyFormCommand,
} from "./components";
import { useGetCompany, useGetCompanyContacts } from "./hooks";

export function CompanyDetailsPage() {
	const { id } = useParams({ from: "/app/companies/$id" });
	const editRef = useRef<EditCompanyFormCommand>(null);

	const companyQuery = useGetCompany(id);
	const contactsQuery = useGetCompanyContacts(id);

	if (!id) {
		return (
			<div className="company-details">
				<div className="company-details-empty">Company not found.</div>
			</div>
		);
	}

	if (companyQuery.isLoading) {
		return (
			<div className="company-details">
				<div className="company-details-loading">Loading company...</div>
			</div>
		);
	}

	if (companyQuery.isError || !companyQuery.data) {
		return (
			<div className="company-details">
				<div className="company-details-error">Unable to load company.</div>
			</div>
		);
	}

	const company = companyQuery.data;

	const contacts: CompanyContact[] = contactsQuery.data ?? [];

	return (
		<>
			<DataDetails>
				<DetailsHeader
					name={company.name}
					website={company.website}
					onEdit={() => editRef.current?.edit(company.id)}
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
					void companyQuery.refetch();
				}}
			/>
		</>
	);
}
