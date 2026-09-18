import clsx from "clsx";
import { CalendarClock, Pencil, Plus } from "lucide-react";
import type { OpportunityProjection } from "#/api/models";
import { formatDateTime } from "#/utlis";
import { ActionButton, Button } from "@/components/ui";

interface OpportunityNextActionProps {
	opportunity: OpportunityProjection;
	onAdd: () => void;
	onEdit: () => void;
}

export function OpportunityNextAction({ opportunity, onAdd, onEdit }: OpportunityNextActionProps) {
	const hasFollowUp = Boolean(opportunity.followUpActionId);

	const isOverdue =
		Boolean(opportunity.followUpDateTime) &&
		new Date(opportunity.followUpDateTime as string).getTime() < Date.now();

	return (
		<section className="data-details-section">
			<div className="data-details-section-header">
				<div>
					<h2>Next action</h2>
					<p>Planned follow up with the client</p>
				</div>

				{hasFollowUp ? (
					<div className="toolbar-right">
						<ActionButton title="Add follow up" onClick={onAdd}>
							<Plus size={15} />
						</ActionButton>

						<ActionButton title="Edit follow up" onClick={onEdit}>
							<Pencil size={15} />
						</ActionButton>
					</div>
				) : null}
			</div>

			{hasFollowUp ? (
				<div className="sales-follow-up">
					<div className={clsx("sales-follow-up-date", { "sales-follow-up-overdue": isOverdue })}>
						<CalendarClock size={15} />
						<span>{formatDateTime(opportunity.followUpDateTime)}</span>
						{isOverdue ? <span className="sales-follow-up-badge">overdue</span> : null}
					</div>

					<p className="sales-follow-up-content">{opportunity.followUpContent}</p>
				</div>
			) : (
				<div className="data-details-empty">
					<div>No follow up scheduled.</div>

					<Button className="mt-3" onClick={onAdd}>
						<CalendarClock size={16} />
						Schedule follow up
					</Button>
				</div>
			)}
		</section>
	);
}
