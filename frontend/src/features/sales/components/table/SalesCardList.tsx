import { useRef } from "react";
import type { OpportunityProjection } from "#/api/models";

import { DetailItem } from "#/components/ui";
import { MessagePreview } from "#/components/ui/MessagePreview";
import type { CardListProps } from "#/types";
import { formatDateTimeIntl, formatSalary } from "#/utlis";
import type { SalesActionRef, SalesActionTypes } from "../forms";
import SalesActionDrawers from "../forms/SalesActionDrawers";
import { SalesStageBadge } from "../sales-stage-badge";
import { SalesActions } from "./SalesActions";
export function SalesCardList({ items, onRefresh }: CardListProps<OpportunityProjection>) {
	const salesRef = useRef<SalesActionRef>(null);

	const handleAction = (action: SalesActionTypes, item: OpportunityProjection): void => {
		if (action === "change-stage") {
			salesRef.current?.changeStage({ id: item.id, stage: item.stage, title: item.title });
			return;
		}
		salesRef?.current?.onAction(item.id, action);
	};

	return (
		<div className="data-mobile-view">
			{items.map<React.ReactNode>((item) => (
				<div
					key={item.id}
					className="flex flex-col gap-3 pb-5 border mb-5 mt-5  shadow-subtle card-border"
				>
					<div className="data-detail-item  flex flex-row">
						<div className="grow">
							<dt>Title</dt>
							<dd>{item.title}</dd>
						</div>
						<SalesActions
							onAction={(val) => handleAction(val, item)}
							mode="table"
							opportunityId={item.id}
						/>
					</div>

					<dl className="data-details-list">
						<DetailItem label="Stage">
							<SalesStageBadge stage={item.stage} />
						</DetailItem>

						<DetailItem label="Company">{item.company.name}</DetailItem>

						<DetailItem label="Exepcetd value">
							{formatSalary(item.expectedValue as number)}
						</DetailItem>
						<DetailItem label="Company">{item.company.name}</DetailItem>

						<DetailItem label="Responsible">
							<div className="flex flex-col">
								<span>{item.responsible?.fullname ?? ""}</span>
								<a href={`mailto:${item.responsible?.email}`}>{item.responsible?.email}</a>
							</div>
						</DetailItem>
						<DetailItem label="Expected close date">
							{item.expectedCloseDate ? formatDateTimeIntl(item.expectedCloseDate) : "-"}
						</DetailItem>
						<DetailItem label="Created by">{item.createdBy.fullname}</DetailItem>
						<DetailItem label="Created at">{formatDateTimeIntl(item.createdAt)}</DetailItem>
						<DetailItem label="Modified at">
							{item.modifiedAt && formatDateTimeIntl(item.modifiedAt)}
						</DetailItem>
						<DetailItem label="Modified by">{item.modifiedBy?.fullname}</DetailItem>
					</dl>
					<DetailItem label="Description">
						<MessagePreview message={item.description} />
					</DetailItem>
				</div>
			))}
			<SalesActionDrawers ref={salesRef} onSuccess={onRefresh} />
		</div>
	);
}
