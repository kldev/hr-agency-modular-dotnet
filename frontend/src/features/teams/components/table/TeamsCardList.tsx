import type { TeamProjection } from "#/api/models";
import { DetailItem } from "#/components/ui";
import { formatDateTimeIntl } from "#/utlis";
import { teamRoles } from "../../types";

interface TeamsCardListProps {
	items: TeamProjection[];
}

export function TeamsCardList({ items }: TeamsCardListProps) {
	return (
		<div className="data-mobile-view">
			{items.map<React.ReactNode>((item) => (
				<div
					key={item.id}
					className="flex flex-col gap-3 pb-5 border mb-5 mt-5  shadow-subtle card-border"
				>
					<div className="data-detail-item  flex flex-row">
						<div className="grow">
							<dt>Team</dt>
							<dd>{item.name}</dd>
						</div>
					</div>

					<dl className="data-details-list">
						<DetailItem label="Members">{item.members.length}</DetailItem>

						<DetailItem label="Roster">
							{item.members
								.map(
									(member) =>
										`${member.user.fullname ?? member.user.email} (${teamRoles[member.role]})`,
								)
								.join(", ")}
						</DetailItem>

						<DetailItem label="Created at">{formatDateTimeIntl(item.createdAt)}</DetailItem>
						<DetailItem label="Created by">{item.createdBy?.fullname}</DetailItem>
					</dl>
				</div>
			))}
		</div>
	);
}
