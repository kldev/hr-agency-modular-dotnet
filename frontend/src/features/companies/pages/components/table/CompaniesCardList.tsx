import { useRef } from "react";
import type { CompanyProjection } from "#/api/models";
import { DetailItem } from "#/components/ui";
import {
	type CompanyContactCommand,
	CompanyContactDrawer,
} from "#/features/company-contacts/components";
import { formatDateTimeIntl } from "#/utlis";
import type { EditCompanyFormCommand } from "../CompanyFormCommand";
import { EditCompanyDrawer } from "../edit";
import { CompanyActions } from "./CompanyActions";

interface CompaniesCardListProps {
	companies: CompanyProjection[];
}

export function CompaniesCardList({ companies }: CompaniesCardListProps) {
	const editRef = useRef<EditCompanyFormCommand>(null);
	const contactRef = useRef<CompanyContactCommand>(null);

	return (
		<div className="data-mobile-view">
			{companies.map<React.ReactNode>((application) => (
				<div
					key={application.id}
					className="flex flex-col gap-3 pb-5 border mb-5 mt-5  shadow-subtle card-border"
				>
					<div className="data-detail-item  flex flex-row">
						<div className="grow">
							<dt>Name</dt>
							<dd>{application.name}</dd>
						</div>
						<CompanyActions
							id={application.id}
							onEdit={() => {
								editRef.current?.edit(application.id);
							}}
							onAddContact={() => {
								contactRef?.current?.create(application.id);
							}}
						></CompanyActions>
					</div>

					<dl className="data-details-list">
						<DetailItem label="Industry">{application.industry}</DetailItem>

						<DetailItem label="Country">{application.countryCode}</DetailItem>
						<DetailItem label="Tax">{application.taxId}</DetailItem>
						<DetailItem label="Registration number">{application.registrationNumber}</DetailItem>

						{application.contact ? (
							<DetailItem label={application.contact?.fullname ?? ""}>
								<div className="flex flex-col">
									<a href={`mailto:${application.contact?.email}`}>{application.contact?.email}</a>
									<a href={`tel:${application.contact?.phone}`}>{application.contact?.phone}</a>
									<span>{application.contact?.jobTitle}</span>
								</div>
							</DetailItem>
						) : null}

						<DetailItem label="Created by">
							<div>
								<span>{application.createdBy?.fullname}</span>
							</div>
							{formatDateTimeIntl(application.createdAt)}
						</DetailItem>
						{application?.modifiedAt ? (
							<DetailItem label="Modified">
								<div>
									<span>{application.modifiedBy?.fullname}</span>
								</div>
								<span>{formatDateTimeIntl(application?.modifiedAt ?? "")}</span>
							</DetailItem>
						) : null}
					</dl>
					{application?.website.length ? (
						<DetailItem label="Website">{application.website}</DetailItem>
					) : null}
				</div>
			))}
			<EditCompanyDrawer ref={editRef} onSuccess={() => {}} />
			<CompanyContactDrawer ref={contactRef} onSuccess={() => {}} />
		</div>
	);
}
