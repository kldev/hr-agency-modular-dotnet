import { useRef } from "react";
import type { CandidateProjection } from "#/api/models";
import { CandidateSourceBadge, DetailItem, DetailsListSection } from "#/components/ui";
import {
	type AddTagCommand,
	AddTagsDrawer,
} from "#/features/applications/pages/components/forms/add-tag";
import { formatDateTimeIntl } from "#/utlis";
import { EditCandidateDrawer, type EditCandidateFormCommand } from "../form";
import { CandidateActions } from "./CandidateActions";
import type { CanidateActions } from "./CandidatesTableColumns";

interface CandidatesCardListProps {
	items: CandidateProjection[];
	onRefresh: () => void;
}

export function CandidatesCardList({ items, onRefresh }: CandidatesCardListProps) {
	const formRef = useRef<EditCandidateFormCommand>(null);
	const tagRef = useRef<AddTagCommand>(null);

	const actionsHandler: CanidateActions = {
		onEdit: (it) => formRef.current?.edit(it.id),
		onTag: (it) => tagRef.current?.addTag(it.id, it.fullName, "candidate"),
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
						<CandidateActions
							id={item.id}
							onTag={() => actionsHandler.onTag(item)}
							onEdit={() => actionsHandler.onEdit(item)}
						></CandidateActions>
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
						<DetailItem label="Modified at">
							{item.modifiedAt && formatDateTimeIntl(item.modifiedAt)}
						</DetailItem>
						<DetailItem label="Modified by">{item.modifiedBy?.fullname}</DetailItem>
					</dl>
					<div className="data-content-lists data-details-section-bg-none">
						<DetailsListSection
							title="Tags"
							items={item.tags.flatMap((z) => z.name) ?? []}
							className="short-items-section"
							onAdd={() => {
								tagRef.current?.addTag(item.id, item.fullName, "candidate");
							}}
						/>
					</div>
				</div>
			))}
			<EditCandidateDrawer ref={formRef} onSuccess={() => onRefresh()} />
			<AddTagsDrawer ref={tagRef} onSuccess={() => onRefresh()} />
		</div>
	);
}
