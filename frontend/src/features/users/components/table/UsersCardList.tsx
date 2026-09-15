import type { UserProjection } from "#/api/models";
import { DetailItem, EmailItem, PhoneItem } from "#/components/ui";

import { formatDateTimeIntl } from "#/utlis";

interface UsersCardListProps {
	items: UserProjection[];
	showOrg?: boolean;
}

export function UsersCardList({ items, showOrg }: UsersCardListProps) {
	return (
		<div className="data-mobile-view">
			{items.map<React.ReactNode>((item) => (
				<div
					key={item.id}
					className="flex flex-col gap-3 pb-5 border mb-5 mt-5  shadow-subtle card-border"
				>
					<div className="data-detail-item  flex flex-row">
						<div className="grow">
							<dt>Full name</dt>
							<dd>{item.fullName}</dd>
						</div>
					</div>

					<dl className="data-details-list">
						<EmailItem email={item.email} />

						<PhoneItem phone={item.phone} />

						<DetailItem label="Role">{item.role}</DetailItem>
						{showOrg ? (
							<DetailItem label="Organization">{item.organization.name}</DetailItem>
						) : (
							<DetailItem label=""> </DetailItem>
						)}

						<DetailItem label="Created at">{formatDateTimeIntl(item.createdAt)}</DetailItem>
						<DetailItem label="Created by">{item.createdBy?.fullname}</DetailItem>
						<DetailItem label="Modified at">
							{item.modifiedAt && formatDateTimeIntl(item.modifiedAt)}
						</DetailItem>
						<DetailItem label="Modified by">{item.modifiedBy?.fullname}</DetailItem>
					</dl>
				</div>
			))}
		</div>
	);
}
