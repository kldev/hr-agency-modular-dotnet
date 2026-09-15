import { useRef } from "react";
import type { JobApplicationProjection } from "#/api/models";
import {
	ApplicationBadge,
	CandidateSourceBadge,
	DetailItem,
	DetailsListSection,
	EmailItem,
	PhoneItem,
} from "#/components/ui";
import { formatDateTimeIntl } from "#/utlis";
import { ApplicationsActionDrawers, type JobApplicationsRef } from "../forms";
import { ApplicationsActions } from "./ApplicationsActions";
import type { Actions } from "./ApplicationsTableColumns";

interface ApplicationCardListProps {
	applications: JobApplicationProjection[];
	onRefresh: () => void;
}

export function ApplicationCardList({ applications, onRefresh }: ApplicationCardListProps) {
	const formRef = useRef<JobApplicationsRef>(null);

	const handleActions: Actions = {
		onAction: (action, item) => {
			formRef.current?.update(item.id, action, item.status, item.applicantFullName);
		},
	};
	return (
		<div className="data-mobile-view">
			{applications.map<React.ReactNode>((application) => (
				<div
					key={application.id}
					className="flex flex-col gap-3 pb-5 border mb-5 mt-5  shadow-subtle card-border"
				>
					<div className="data-detail-item  flex flex-row">
						<div className="grow">
							<dt>Full name</dt>
							<dd>{application.applicantFullName}</dd>
						</div>
						<ApplicationsActions
							id={application.id}
							onAction={(action) => handleActions.onAction(action, application)}
						></ApplicationsActions>
					</div>

					<dl className="data-details-list">
						<EmailItem email={application.applicantEmail} />

						<PhoneItem phone={application.applicantPhone} />
						<DetailItem label="Source">
							<CandidateSourceBadge source={application.source} />
						</DetailItem>
						<DetailItem label="Status">
							<ApplicationBadge status={application.status} />
						</DetailItem>
						<DetailItem label="Job post">{application.jobPostTitle}</DetailItem>
						<DetailItem label="Company">{application.company.name}</DetailItem>
						<DetailItem label="Created at">{formatDateTimeIntl(application.createdAt)}</DetailItem>
						<DetailItem label="">-</DetailItem>
					</dl>
					<div className="data-content-lists data-details-section-bg-none">
						<DetailsListSection
							title="Tags"
							items={[...application.tags.flatMap((z) => z.name)]}
							className="short-items-section bg-none!"
							onAdd={() => {
								formRef?.current?.update(
									application.id,
									"tag",
									undefined,
									application.applicantFullName,
								);
							}}
						/>
					</div>
				</div>
			))}
			<ApplicationsActionDrawers ref={formRef} onSuccess={onRefresh} />
		</div>
	);
}
