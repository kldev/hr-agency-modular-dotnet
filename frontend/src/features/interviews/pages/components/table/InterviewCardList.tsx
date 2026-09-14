import { useRef } from "react";
import type { InterviewProjection } from "#/api/models";
import {
	DetailItem,
	InterviewFormatBadge,
	InterviewStatusBadge,
	InterviewTypeBadge,
} from "#/components/ui";
import { MessagePreview } from "#/components/ui/MessagePreview";
import { formatDateTimeIntl } from "#/utlis";
import {
	InterviewActionDrawers,
	type InterviewActionsRef,
	type InterviewActionsType,
} from "../forms";
import { InterviewActions } from "./InterviewActions";
import type { Actions } from "./InterviewsTableColumns";

interface InterviewCardListProps {
	interviews: InterviewProjection[];
	onRefresh: () => void;
}

export function InterviewCardList({ interviews, onRefresh }: InterviewCardListProps) {
	const updateRef = useRef<InterviewActionsRef>(null);

	const actionsHandler: Actions = {
		onAction: (action: InterviewActionsType, item: InterviewProjection): void => {
			updateRef?.current?.update(item.id, action);
		},
	};
	return (
		<div className="data-mobile-view">
			{interviews.map((interview) => (
				<div
					key={interview.id}
					className="flex flex-col gap-3 pb-5 border mb-5 mt-5 shadow-subtle card-border"
				>
					<div className="data-detail-item flex flex-row">
						<div className="grow">
							<dt>Applicant</dt>
							<dd>
								{interview.applicantInfo.firstName} {interview.applicantInfo.lastName}
							</dd>
						</div>

						<InterviewActions
							applicationId={interview.applicationId}
							onAction={(action) => actionsHandler.onAction(action, interview)}
						/>
					</div>
					<dl className="data-details-list">
						<DetailItem label="Schedule">
							<div className="flex flex-col">
								<span>{formatDateTimeIntl(interview.scheduleAt)}</span>
								<span className="text-muted-foreground">{interview.timezone}</span>
							</div>
						</DetailItem>

						<DetailItem label="Contact">
							<div className="flex flex-col">
								<a href={`mailto:${interview.applicantInfo.email}`}>
									{interview.applicantInfo.email || "-"}
								</a>
								{interview.applicantInfo.phoneNumber ? (
									<a href={`tel:${interview.applicantInfo?.phoneNumber}`}>
										{interview.applicantInfo?.phoneNumber}
									</a>
								) : null}
							</div>
						</DetailItem>

						<DetailItem label="Job post">{interview.jobPostTitle}</DetailItem>

						<DetailItem label="Info">
							<div className="flex flex-row gap-2">
								<InterviewStatusBadge status={interview.status} />
								<InterviewTypeBadge status={interview.interviewType} />
								<InterviewFormatBadge format={interview.format} />
							</div>
						</DetailItem>

						<DetailItem label="Interviewer">
							<div className="flex flex-col">
								<span>{interview.interviewer?.fullname}</span>
								<a href={`mailto:${interview.interviewer?.email}`}>
									{interview.interviewer?.email}
								</a>
							</div>
						</DetailItem>

						<DetailItem label="Location">{interview.location || "-"}</DetailItem>

						{interview.meetingUrl ? (
							<DetailItem label="Meeting">
								<a href={interview.meetingUrl} target="_blank" rel="noreferrer">
									Join meeting
								</a>
							</DetailItem>
						) : null}

						<DetailItem label="Created by">
							<div className="flex flex-col">
								<span>{interview.createdBy?.fullname}</span>
								<span>{formatDateTimeIntl(interview.createdAt)}</span>
							</div>
						</DetailItem>

						{interview.modifiedBy ? (
							<DetailItem label="Modified">
								<div className="flex flex-col">
									<span>{interview.modifiedBy.fullname}</span>
									{interview.modifiedAt ? (
										<span>{formatDateTimeIntl(interview.modifiedAt)}</span>
									) : null}
								</div>
							</DetailItem>
						) : null}
					</dl>
					{interview.note ? (
						<DetailItem label="Note">
							<MessagePreview message={interview.note} />
						</DetailItem>
					) : null}
				</div>
			))}
			<InterviewActionDrawers ref={updateRef} onSuccess={onRefresh} />
		</div>
	);
}
