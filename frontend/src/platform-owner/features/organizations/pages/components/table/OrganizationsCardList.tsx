import type { OrganizationProjection } from "#/api/models";
import { DetailItem, DetailsListSection, EmailItem, PhoneItem } from "#/components/ui";
import { formatDateTimeIntl } from "#/utlis";

interface OrganizationsCardListProps {
	items: OrganizationProjection[];
}

export function OrganizationsCardList({ items }: OrganizationsCardListProps) {
	return (
		<div className="data-mobile-view">
			{items.map<React.ReactNode>((item) => (
				<div
					key={item.id}
					className="flex flex-col gap-3 pb-5 border mb-5 mt-5  shadow-subtle card-border"
				>
					<div className="data-detail-item  flex flex-row">
						<div className="grow">
							<dt>Name</dt>
							<dd>{item.name}</dd>
						</div>
					</div>

					<dl className="data-details-list">
						<EmailItem email={item.info?.email ?? ""} />

						<PhoneItem phone={item.info?.phone} />

						<DetailItem label="Slug">{item.slug}</DetailItem>
						<DetailItem label="Location">{item.info?.location ?? "-"}</DetailItem>

						<DetailItem label="Created at">{formatDateTimeIntl(item.createdAt)}</DetailItem>
						<DetailItem label="Modified at">
							{item.modifiedAt && formatDateTimeIntl(item.modifiedAt)}
						</DetailItem>
					</dl>
					<DetailItem label="Website">{item.info?.website ?? "-"}</DetailItem>
					<DetailsListSection
						title="Domains"
						items={item.emailDomains ?? []}
						className="short-items-section"
					/>
				</div>
			))}
		</div>
	);
}
