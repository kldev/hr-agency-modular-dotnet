import { useRef } from "react";
import type { CompanyProjection } from "#/api/models";
import { DetailItem } from "#/components/ui";
import {
	type CompanyContactCommand,
	CompanyContactDrawer,
} from "#/features/company-contacts/components";
import { CreateOpportunityDrawer, type CreateOpportunityRef } from "#/features/sales/components";
import { formatDateTimeIntl } from "#/utlis";
import type { EditCompanyFormCommand } from "../CompanyFormCommand";
import { EditCompanyDrawer } from "../edit";
import { CompanyActions } from "./CompanyActions";

interface CompaniesCardListProps {
	companies: CompanyProjection[];
	onRefresh: () => void;
}

export function CompaniesCardList({ companies, onRefresh }: CompaniesCardListProps) {
	const editRef = useRef<EditCompanyFormCommand>(null);
	const contactRef = useRef<CompanyContactCommand>(null);
	const oppRef = useRef<CreateOpportunityRef>(null);

	return (
		<div className="data-mobile-view">
			{companies.map<React.ReactNode>((company) => (
				<div
					key={company.id}
					className="flex flex-col gap-3 pb-5 border mb-5 mt-5  shadow-subtle card-border"
				>
					<div className="data-detail-item  flex flex-row">
						<div className="grow">
							<dt>Name</dt>
							<dd>{company.name}</dd>
						</div>
						<CompanyActions
							id={company.id}
							onEdit={() => {
								editRef.current?.edit(company.id);
							}}
							onAddContact={() => {
								contactRef?.current?.create(company.id);
							}}
							onAddOpportunity={() => {
								oppRef?.current?.create({
									companyId: company.id,
									companyName: company.name,
								});
							}}
						></CompanyActions>
					</div>

					<dl className="data-details-list">
						<DetailItem label="Industry">{company.industry}</DetailItem>

						<DetailItem label="Country">{company.countryCode}</DetailItem>
						<DetailItem label="Tax">{company.taxId}</DetailItem>
						<DetailItem label="Registration number">{company.registrationNumber}</DetailItem>

						{company.contact ? (
							<DetailItem label={company.contact?.fullname ?? ""}>
								<div className="flex flex-col">
									<a href={`mailto:${company.contact?.email}`}>{company.contact?.email}</a>
									<a href={`tel:${company.contact?.phone}`}>{company.contact?.phone}</a>
									<span>{company.contact?.jobTitle}</span>
								</div>
							</DetailItem>
						) : null}

						<DetailItem label="Created by">
							<div>
								<span>{company.createdBy?.fullname}</span>
							</div>
							{formatDateTimeIntl(company.createdAt)}
						</DetailItem>
						{company?.modifiedAt ? (
							<DetailItem label="Modified">
								<div>
									<span>{company.modifiedBy?.fullname}</span>
								</div>
								<span>{formatDateTimeIntl(company?.modifiedAt ?? "")}</span>
							</DetailItem>
						) : null}
					</dl>
					{company?.website.length ? (
						<DetailItem label="Website">{company.website}</DetailItem>
					) : null}
				</div>
			))}
			<EditCompanyDrawer ref={editRef} onSuccess={onRefresh} />
			<CompanyContactDrawer ref={contactRef} onSuccess={() => {}} />
			<CreateOpportunityDrawer ref={oppRef} onSuccess={() => {}} />
		</div>
	);
}
