import {
	CalendarDays,
	type LucideIcon,
	Mail,
	MessageSquare,
	Phone,
	Presentation,
} from "lucide-react";
import type { ActivityProjection, SalesActivityType } from "#/api/models";
import { MessagePreview } from "#/components/ui/MessagePreview";
import { formatDateTime } from "#/utlis";
import { Button, LoadMore } from "@/components/ui";
import { useGetActivitiesSlice } from "../../hooks";
import { activityTypeOptions } from "../../types";

interface OpportunityActivityProps {
	opportunityId: string;
	onLogActivity: () => void;
}

const activityIcons: Record<SalesActivityType, LucideIcon> = {
	Call: Phone,
	Email: Mail,
	Meeting: CalendarDays,
	Note: MessageSquare,
	Presentation: Presentation,
	Other: MessageSquare,
};

function ActivityItem({ activity }: { activity: ActivityProjection }) {
	const Icon = activityIcons[activity.activityType];

	return (
		<article className="sales-activity-item">
			<div className="sales-activity-icon">
				<Icon size={16} />
			</div>

			<div className="sales-activity-content">
				<div className="sales-activity-heading">
					<h3>
						{activityTypeOptions[activity.activityType]} · {activity.createdBy?.fullname}
					</h3>

					<time>{formatDateTime(activity.createdAt)}</time>
				</div>

				<MessagePreview message={activity.note} />
			</div>
		</article>
	);
}

export function OpportunityActivity({ opportunityId, onLogActivity }: OpportunityActivityProps) {
	const query = useGetActivitiesSlice(opportunityId);

	const activities = query.data?.pages.flatMap((page) => page.content) ?? [];

	return (
		<section className="data-details-section">
			<div className="data-details-section-header">
				<div>
					<h2>Activity {activities.length ? `(${activities.length})` : ""}</h2>
					<p>Recent communication and sales activity</p>
				</div>

				<div className="toolbar-right">
					<Button onClick={onLogActivity}>
						<MessageSquare size={16} />
						Log activity
					</Button>
				</div>
			</div>

			{query.isLoading ? <div className="data-details-loading">Loading ...</div> : null}

			{query.isError ? (
				<div className="data-details-error">Unable to load the activity.</div>
			) : null}

			{!query.isLoading && !query.isError && activities.length === 0 ? (
				<div className="data-details-empty">No activity yet.</div>
			) : null}

			{activities.length ? (
				<div className="sales-activity-list">
					{activities.map((activity) => (
						<ActivityItem key={activity.id} activity={activity} />
					))}
				</div>
			) : null}

			<LoadMore
				hasNext={Boolean(query.hasNextPage)}
				loading={query.isFetchingNextPage}
				onClick={() => query.fetchNextPage()}
			/>
		</section>
	);
}
