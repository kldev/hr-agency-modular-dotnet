import { MessagePreview } from "#/components/ui/MessagePreview";
import { formatDate, formatSalary } from "#/utlis";
import { AuditInformation, DataDetails, DetailsHeader, DetailsLoading } from "@/components/ui";
import { DataDetailsLayout, DetailItem } from "@/components/ui/details/DataDetails";
import { SalesStageBadge } from "../components";
import { useGetOpportunity } from "../hooks";
import { salesStageOptions } from "../types";

const OpportunityDetailsPage: React.FC<{ id: string }> = ({ id }) => {
	const query = useGetOpportunity(id);

	if (!id || query.isLoading || query.isError || !query.data) {
		return (
			<DetailsLoading id={id} isLoading={query.isLoading} isError={query.isError || !query.data} />
		);
	}

	const opportunity = query.data;

	return (
		<DataDetails>
			<DetailsHeader
				name={opportunity.title}
				detailsAddons={
					<div className="data-details-header-meta">
						<SalesStageBadge stage={opportunity.stage} />
					</div>
				}
			/>

			<DataDetailsLayout
				main={
					<section className="data-details-section">
						<div className="data-details-section-header">
							<div>
								<h2>Opportunity</h2>
								<p>Sales opportunity information</p>
							</div>
						</div>

						<dl className="data-details-list">
							<DetailItem label="Title">{opportunity.title || "-"}</DetailItem>

							<DetailItem label="Company">{opportunity.company?.name || "-"}</DetailItem>

							<DetailItem label="Value">
								{`${formatSalary(Number(opportunity.expectedValue))} ${opportunity.currencyCode}`}
							</DetailItem>

							<DetailItem label="Expected close date">
								{formatDate(opportunity.expectedCloseDate)}
							</DetailItem>

							<DetailItem label="Stage">{salesStageOptions[opportunity.stage]}</DetailItem>

							<DetailItem label="Hot lead">{opportunity.isHotLead ? "Yes" : "No"}</DetailItem>

							{opportunity.stage === "Lost" ? (
								<DetailItem label="Lost reason">{opportunity.lostReason || "-"}</DetailItem>
							) : null}
						</dl>

						<DetailItem label="Description">
							<MessagePreview message={opportunity.description} />
						</DetailItem>
					</section>
				}
				sidebar={
					<>
						<section className="data-details-section">
							<div className="data-details-section-header">
								<div>
									<h2>Responsible</h2>
									<p>Person in charge of the opportunity</p>
								</div>
							</div>

							<dl className="data-details-list">
								<DetailItem label="Name">{opportunity.responsible?.fullname || "-"}</DetailItem>

								<DetailItem label="Email">
									<a href={`mailto:${opportunity.responsible?.email}`}>
										{opportunity.responsible?.email}
									</a>
								</DetailItem>
							</dl>
						</section>

						<AuditInformation
							createdAt={opportunity.createdAt}
							createdBy={opportunity.createdBy}
							modifiedAt={opportunity.modifiedAt}
							modifiedBy={opportunity.modifiedBy}
						/>
					</>
				}
			/>
		</DataDetails>
	);
};

export default OpportunityDetailsPage;
