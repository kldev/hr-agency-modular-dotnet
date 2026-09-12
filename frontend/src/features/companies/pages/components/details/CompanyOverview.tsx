import type { CompanyProjection } from "@/api/models";
import { DetailItem, DetailOverviewHeader } from "@/components/ui";
import { formatDateTime } from "@/utlis/dateUtils";

interface CompanyOverviewProps {
	company: CompanyProjection;
}

export function CompanyOverview({ company }: CompanyOverviewProps) {
	return (
		<div className="data-overview">
			<DetailOverviewHeader title="Company details" description="Basic company information." />

			<dl className="data-details-list">
				<DetailItem label="Company name">{company.name}</DetailItem>

				<DetailItem label="Industry">{company.industry}</DetailItem>

				<DetailItem label="Tax ID">{company.taxId}</DetailItem>

				<DetailItem label="Country">{company.countryCode}</DetailItem>

				<DetailItem label="Website">
					{company.website ? (
						<a href={company.website} target="_blank" rel="noreferrer">
							{company.website}
						</a>
					) : null}
				</DetailItem>

				<DetailItem label="Active job posts">
					<span className="data-detail-number">{company.activeJobsPostCount}</span>
				</DetailItem>

				<DetailItem label="Applicants">
					<span className="data-detail-number">{company.applicantsCount}</span>
				</DetailItem>
				<DetailItem label="Registration number">{company.registrationNumber}</DetailItem>
				<DetailItem label="Created">{formatDateTime(company.createdAt)}</DetailItem>

				<DetailItem label="Updated">{formatDateTime(company.modifiedAt)}</DetailItem>
			</dl>
		</div>
	);
}
