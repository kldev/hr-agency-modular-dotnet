import { useRef } from "react";
import type { JobPostResponse } from "#/api/models";
import { formatSalary } from "#/components/labels/enum-labels";
import { DetailItem, JobPostsBadge } from "#/components/ui";
import {
	type CreateJobApplicationsCommand,
	CreateJobApplicationsDrawer,
} from "#/features/applications/pages/components";
import { formatDateTimeIntl } from "#/utlis";
import {
	type ChangeJobPostRecruiterCommand,
	type ChangeJobPostStatusCommand,
	ChangeJobPostStatusDrawer,
	ChangeRecruiterDrawer,
	type PostToChannelCommand,
	PostToChannelDrawer,
} from "../forms";
import { JobPostsActions } from "./JobPostsActions";
import type { Actions } from "./JobPostsTableColumns";

interface JobPostCardListProps {
	jobPosts: JobPostResponse[];
	onRefresh: () => void;
}

export function JobPostCardList({ jobPosts, onRefresh }: JobPostCardListProps) {
	const applicationRef = useRef<CreateJobApplicationsCommand>(null);
	const statusRef = useRef<ChangeJobPostStatusCommand>(null);
	const channelRef = useRef<PostToChannelCommand>(null);
	const recruiterRef = useRef<ChangeJobPostRecruiterCommand>(null);

	const actionsHandler: Actions = {
		onAddApplication: (it) => {
			applicationRef.current?.create(it.id, it.title);
		},
		onChangeStatus: (it) => {
			statusRef.current?.changeStatus(it.id);
		},
		onPostToChannel: (it) => {
			channelRef.current?.postToChannel(it.id);
		},
		onChangeRecruiter: (it) => {
			recruiterRef.current?.changeRecruiter(it.id, it.recruiterId);
		},
	};
	return (
		<div className="data-mobile-view">
			{jobPosts.map<React.ReactNode>((jobPost) => (
				<div
					key={jobPost.id}
					className="flex flex-col gap-3 pb-5 border mb-5 mt-5  shadow-subtle card-border"
				>
					<div className="data-detail-item  flex flex-row">
						<div className="grow">
							<dt>Title</dt>
							<dd>{jobPost.title}</dd>
						</div>
						<JobPostsActions
							id={jobPost.id}
							onAddApplication={() => actionsHandler.onAddApplication(jobPost)}
							onChangeStatus={() => actionsHandler.onChangeStatus(jobPost)}
							onPostToChannel={() => actionsHandler.onPostToChannel(jobPost)}
							onChangeRecruiter={() => actionsHandler.onChangeRecruiter(jobPost)}
						></JobPostsActions>
					</div>

					<dl className="data-details-list">
						<DetailItem label="Status">
							<JobPostsBadge status={jobPost.status} />
						</DetailItem>

						<DetailItem label="Language">{jobPost.languageCode}</DetailItem>
						<DetailItem label="Type">{jobPost.employmentType}</DetailItem>
						<DetailItem label="Work">{jobPost.workMode}</DetailItem>
						<DetailItem label="Location">{jobPost.location}</DetailItem>

						<DetailItem label="Salary">
							{formatSalary(jobPost.salaryMin, jobPost.salaryMax, jobPost.currencyCode)}
						</DetailItem>
						<DetailItem label="Company">{jobPost.company.name}</DetailItem>
						{jobPost.recruiter ? (
							<DetailItem label="Responsible">
								<div className="flex flex-col">
									<span>{jobPost.recruiter?.fullname ?? ""}</span>
									<a href={`mailto:${jobPost.recruiter?.email}`}>{jobPost.recruiter?.email}</a>
								</div>
							</DetailItem>
						) : null}

						<DetailItem label="Created by">
							<div>
								<span>{jobPost.createdBy?.fullname}</span>
							</div>
							{formatDateTimeIntl(jobPost.createdAt)}
						</DetailItem>
						{jobPost?.modifiedBy ? (
							<DetailItem label="Modified">
								<div>
									<span>{jobPost.modifiedBy?.fullname}</span>
								</div>
								{/* <span>{formatDateTimeIntl(jobPost?.modifiedAt ?? "")}</span> */}
							</DetailItem>
						) : null}
					</dl>
				</div>
			))}
			<CreateJobApplicationsDrawer ref={applicationRef} onSuccess={onRefresh} />
			<PostToChannelDrawer ref={channelRef} onSuccess={onRefresh} />
			<ChangeJobPostStatusDrawer ref={statusRef} onSuccess={onRefresh} />
			<ChangeRecruiterDrawer ref={recruiterRef} onSuccess={onRefresh} />
		</div>
	);
}
