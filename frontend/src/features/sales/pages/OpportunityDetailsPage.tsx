import { useQueryClient } from "@tanstack/react-query";
import { useRef } from "react";
import type { OpportunityStage } from "#/api/models";
import { MessagePreview } from "#/components/ui/MessagePreview";
import { formatDate, formatSalary } from "#/utlis";
import { salesKeys } from "@/api/query-keys";
import { AuditInformation, DataDetails, DetailsHeader, DetailsLoading } from "@/components/ui";
import { DataDetailsLayout, DetailItem } from "@/components/ui/details/DataDetails";
import { SalesActions, SalesStageBadge } from "../components";
import type { SalesActionRef, SalesActionTypes } from "../components/forms";
import SalesActionDrawers from "../components/forms/SalesActionDrawers";
import { useGetOpportunity } from "../hooks";
import { salesStageOptions } from "../types";
import { OpportunityActivity, OpportunityPipeline } from "./components";
import "./sales-details.css";

const OpportunityDetailsPage: React.FC<{ id: string }> = ({ id }) => {
	const salesRef = useRef<SalesActionRef>(null);
	const client = useQueryClient();

	const query = useGetOpportunity(id);

	if (!id || query.isLoading || query.isError || !query.data) {
		return (
			<DetailsLoading id={id} isLoading={query.isLoading} isError={query.isError || !query.data} />
		);
	}

	const opportunity = query.data;

	const refetch = () => {
		query.refetch();
		client.invalidateQueries({ queryKey: salesKeys.activities(id) });
	};

	const handleAction = (action: SalesActionTypes) => {
		if (action === "change-stage") {
			changeStage();
			return;
		}

		salesRef.current?.onAction(opportunity.id, action);
	};

	const changeStage = (targetStage?: OpportunityStage) => {
		salesRef.current?.changeStage({
			id: opportunity.id,
			stage: opportunity.stage,
			title: opportunity.title,
			targetStage,
		});
	};

	return (
		<DataDetails>
			<DetailsHeader
				name={opportunity.title}
				detailsAddons={
					<div className="data-details-header-meta">
						<SalesStageBadge stage={opportunity.stage} />
					</div>
				}
				extraAdd={
					<SalesActions onAction={handleAction} mode="details" opportunityId={opportunity.id} />
				}
				onEdit={() => {
					salesRef.current?.onAction(opportunity.id, "edit-opportunity");
				}}
			/>

			<OpportunityPipeline
				stage={opportunity.stage}
				lostReason={opportunity.lostReason}
				onStageChange={changeStage}
			/>

			<DataDetailsLayout
				main={
					<>
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

						<OpportunityActivity
							opportunityId={opportunity.id}
							onLogActivity={() => {
								salesRef.current?.onAction(opportunity.id, "log-activity");
							}}
						/>
					</>
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

			<SalesActionDrawers ref={salesRef} onSuccess={refetch} />
		</DataDetails>
	);
};

export default OpportunityDetailsPage;
