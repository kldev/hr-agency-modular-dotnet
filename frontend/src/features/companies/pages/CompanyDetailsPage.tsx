import { useQuery } from "@tanstack/react-query";
import { useRef } from "react";
import { useParams } from "react-router-dom";
import "./components/details/company-details.css";

import { getCompany, getCompanyContacts } from "@/api/endpoints";
import type { CompanyContact } from "@/api/models";
import { DetailsHeader } from "@/components/ui";
import { DataDetails } from "@/components/ui/details/DataDetails";
import {
	CompanyContacts,
	CompanyOverview,
	EditCompanyDrawer,
	type EditCompanyFormCommand,
} from "./components";

export function CompanyDetailsPage() {
	const { id } = useParams<{ id: string }>();
	const editRef = useRef<EditCompanyFormCommand>(null);

	const companyQuery = useQuery({
		queryKey: ["company", id],
		queryFn: ({ signal }) => {
			if (!id) {
				throw new Error("Company id is required");
			}

			return getCompany(id, undefined, signal);
		},
		enabled: Boolean(id),
	});

	const contactsQuery = useQuery({
		queryKey: ["company-contacts", id],
		queryFn: ({ signal }) => {
			if (!id) {
				throw new Error("Company id is required");
			}

			return getCompanyContacts(id, undefined, signal);
		},
		enabled: Boolean(id),
	});

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

				<div className="company-details-grid">
					<section className="data-details-section">
						<CompanyOverview company={company} />
					</section>

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
				</div>
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
