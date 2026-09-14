import { useRef } from "react";
import type { CandidateProjection } from "#/api/models";
import { CandidateSourceBadge, DetailItem, DetailsListSection } from "#/components/ui";

import { formatDateTimeIntl } from "#/utlis";
import type { EditCandidateFormCommand } from "../form";
import { CandidateActions } from "./CandidateActions";

interface CandidatesCardListProps {
	items: CandidateProjection[];
}

export function CandidatesCardList({ items }: CandidatesCardListProps) {
	const formRef = useRef<EditCandidateFormCommand>(null);

	const handleOnEdit = (item: CandidateProjection) => {
		formRef.current?.edit(item.id);
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
							<dt>Full name</dt>
							<dd>{item.fullName}</dd>
						</div>
						<CandidateActions id={item.id} onEdit={() => handleOnEdit(item)}></CandidateActions>
					</div>

					<dl className="data-details-list">
						<DetailItem label="Email">
							<a href={`mailto:${item.email}`}>{item.email}</a>
						</DetailItem>

						<DetailItem label="Phone">
							<a href={`tel:${item.phoneNumber}`}>{item.phoneNumber}</a>
						</DetailItem>

						<DetailItem label="Source">
							<CandidateSourceBadge source={item.source} />
						</DetailItem>

						<DetailItem label="Created at">{formatDateTimeIntl(item.createdAt)}</DetailItem>
						<DetailItem label="Modified at">{item.modifiedAt}</DetailItem>
						<DetailItem label="Modified by">{item.modifiedBy?.fullname}</DetailItem>
					</dl>
					<div className="data-content-lists data-details-section-bg-none">
						<DetailsListSection
							title="Tags"
							items={item.tags.flatMap((z) => z.name) ?? []}
							className="short-items-section"
							onAdd={() => {}}
						/>
					</div>
				</div>
			))}
		</div>
	);
}
