import { Link } from "@tanstack/react-router";
import { Building2, CalendarDays, Flame, User } from "lucide-react";
import type { OpportunityProjection } from "#/api/models";
import { MessagePreview } from "#/components/ui/MessagePreview";
import { formatDate, formatDateTime, formatSalary } from "#/utlis";
import { Kanban } from "@/components/kanban";
import { ItemMark } from "@/components/ui";
import type { SalesActionTypes } from "../forms";
import { SalesActions } from "../table";

interface OpportunityCardProps {
	opportunity: OpportunityProjection;
	onAction: (action: SalesActionTypes) => void;
}

export function OpportunityCard({ opportunity, onAction }: OpportunityCardProps) {
	return (
		<Kanban.Card>
			<div className="sales-card-header">
				<ItemMark name={opportunity.title} />

				<div className="sales-card-identity">
					<Link
						to="/app/sales/opportunities/$id"
						params={{ id: opportunity.id }}
						className="sales-card-title"
					>
						{opportunity.title}
					</Link>

					<div className="sales-card-company">
						<Building2 size={13} />
						<span>{opportunity.company?.name}</span>
					</div>
				</div>

				<SalesActions onAction={onAction} mode="table" opportunityId={opportunity.id} />
			</div>

			<div className="sales-card-main">
				{opportunity.isHotLead ? (
					<span className="sales-card-hot">
						<Flame size={13} />
						Hot lead
					</span>
				) : (
					<span />
				)}

				<strong className="sales-card-value">
					{formatSalary(Number(opportunity.expectedValue))} {opportunity.currencyCode}
				</strong>
			</div>

			<div className="sales-card-details">
				<div className="sales-card-detail">
					<User size={13} />
					<span>{opportunity.responsible?.fullname ?? opportunity.responsible?.email}</span>
				</div>

				<div className="sales-card-detail">
					<CalendarDays size={13} />
					<span>{formatDate(opportunity.expectedCloseDate)}</span>
				</div>
			</div>

			{opportunity.stage === "Lost" && opportunity.lostReason ? (
				<div className="sales-card-lost">
					<MessagePreview message={opportunity.lostReason} maxLength={60} />
				</div>
			) : null}

			<div className="sales-card-footer">
				<span>Updated</span>
				<span>{formatDateTime(opportunity.modifiedAt ?? opportunity.createdAt)}</span>
			</div>
		</Kanban.Card>
	);
}
