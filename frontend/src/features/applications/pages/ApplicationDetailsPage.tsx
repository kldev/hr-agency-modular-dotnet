import { useQueryClient } from "@tanstack/react-query";
import { useRef } from "react";
import { ApplicationTimeline } from "#/features/timeline";
import { applicationKeys } from "@/api/query-keys";

import {
	ApplicationBadge,
	AuditInformation,
	DataDetails,
	DetailsHeader,
	DetailsListSection,
	DetailsLoading,
	type TabDefinition,
	TabPanel,
	Tabs,
} from "@/components/ui";
import { DataDetailsLayout, DetailItem } from "@/components/ui/details/DataDetails";
import { applicationSources } from "../types";
import { ApplicationsActionDrawers, DetailsActions, type JobApplicationsRef } from "./components";
import { NotesList } from "./components/details";
import { useGetApplicationDetails } from "./hooks";

export type ApplicationTab = "details" | "timeline";

export const applicationTabs: readonly ApplicationTab[] = ["details", "timeline"];

const tabs: TabDefinition<ApplicationTab>[] = [
	{ id: "details", label: "Details" },
	{ id: "timeline", label: "Timeline" },
];

interface ApplicationDetailsPageProps {
	id: string;
	tab: ApplicationTab;
	onTabChange: (tab: ApplicationTab) => void;
}

const ApplicationDetailsPage: React.FC<ApplicationDetailsPageProps> = ({
	id,
	tab,
	onTabChange,
}) => {
	const formRef = useRef<JobApplicationsRef>(null);
	const queryClient = useQueryClient();

	const applicationQuery = useGetApplicationDetails(id);

	if (!id || applicationQuery.isLoading || applicationQuery.isError || !applicationQuery.data) {
		return (
			<DetailsLoading
				id={id}
				isLoading={applicationQuery.isLoading}
				isError={applicationQuery.isError || !applicationQuery.data}
			/>
		);
	}

	/* Every action on this page - status, note, tag - is also a line in the timeline. */
	const refetch = () => {
		applicationQuery.refetch();
		void queryClient.invalidateQueries({ queryKey: applicationKeys.timeline(id) });
	};

	const application = applicationQuery.data;

	const tags = application?.tags?.length ? application.tags.map((z) => z.name) : [];

	return (
		<DataDetails>
			<DetailsHeader
				name={application.applicantFullName}
				detailsAddons={
					<div className="data-details-header-meta">
						<ApplicationBadge status={application.status} />

						<span className="data-details-header-info">
							{applicationSources[application.source]}
						</span>
					</div>
				}
				onEdit={() => {
					formRef.current?.update(application.id, "edit");
				}}
				extraAdd={
					<DetailsActions
						onAction={(action) => {
							formRef.current?.update(application.id, action, application.status);
						}}
					/>
				}
			/>

			<Tabs value={tab} tabs={tabs} onChange={onTabChange} label="Application sections" />

			<DataDetailsLayout
				main={
					<TabPanel id={tab}>
						{tab === "details" ? (
							<>
								<section className="data-details-section">
									<div className="data-details-section-header">
										<div>
											<h2>Applicant</h2>
											<p>Candidate contact information</p>
										</div>
									</div>

									<dl className="data-details-list">
										<DetailItem label="First name">{application.applicantFirstName}</DetailItem>

										<DetailItem label="Last name">{application.applicantLastName}</DetailItem>

										<DetailItem label="Email">
											<a href={`mailto:${application.applicantEmail}`}>
												{application.applicantEmail}
											</a>
										</DetailItem>

										<DetailItem label="Phone">
											<a href={`tel:${application.applicantPhone}`}>{application.applicantPhone}</a>
										</DetailItem>

										<DetailItem label="Candidate ID">{application.candidateId}</DetailItem>

										<DetailItem label="Source">{applicationSources[application.source]}</DetailItem>
									</dl>
								</section>
								<section className="data-details-section">
									<div className="data-details-section-header">
										<div>
											<h2>Application</h2>
											<p>Application and recruitment details</p>
										</div>
									</div>

									<dl className="data-details-list">
										<DetailItem label="Job post">{application.jobPostTitle}</DetailItem>

										<DetailItem label="Company">{application.company.name}</DetailItem>

										<DetailItem label="Status">
											<ApplicationBadge status={application.status} />
										</DetailItem>
									</dl>
								</section>

								<NotesList
									id={application.id}
									add={() => {
										formRef?.current?.update(application.id, "add-note");
									}}
								/>
							</>
						) : null}

						{tab === "timeline" ? <ApplicationTimeline jobApplicationId={application.id} /> : null}
					</TabPanel>
				}
				sidebar={
					<>
						<AuditInformation
							createdAt={application.createdAt}
							createdBy={application.createdBy}
							modifiedAt={application.modifiedAt}
							modifiedBy={application.modifiedBy}
						/>
						<div className="data-content-lists ">
							<DetailsListSection
								title="Tags"
								items={tags}
								className="short-items-section"
								onAdd={() => {
									formRef.current?.update(application.id, "tag", undefined, {
										fullName: application.applicantFullName,
										email: application.applicantEmail,
									});
								}}
							/>
						</div>
					</>
				}
			/>

			<ApplicationsActionDrawers
				ref={formRef}
				onSuccess={() => {
					refetch();
				}}
			/>
		</DataDetails>
	);
};

export default ApplicationDetailsPage;
