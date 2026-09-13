import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useRef } from "react";
import { useParams } from "react-router-dom";
import { getJobApplication } from "@/api/endpoints";
import type { CandidateSource } from "@/api/models";
import { ApplicationBadge, DataDetails, DetailsHeader, DetailsListSection } from "@/components/ui";
import { DataDetailsLayout, DetailItem } from "@/components/ui/details/DataDetails";
import {
	type ScheduleInterviewCommand,
	ScheduletInterviewDrawer,
} from "@/features/interviews/pages/components";
import {
	AddJobApplicationNoteDrawer,
	type AddJobApplicationNoteFormCommand,
	ChangeJobApplicationStatusDrawer,
	type ChangeJobApplicationStatusFormCommand,
	DetailsActions,
	type EditApplicantCommand,
	EditApplicantDrawer,
} from "./components";
import { NotesList } from "./components/details";

function formatDate(value?: string | null) {
	if (!value) return "—";

	return new Intl.DateTimeFormat("pl-PL", {
		dateStyle: "medium",
		timeStyle: "short",
	}).format(new Date(value));
}

function getSourceLabel(source: CandidateSource) {
	switch (source) {
		case "PracujPl":
			return "Pracuj.pl";
		case "LinkedIn":
			return "LinkedIn";
		case "Indeed":
			return "Indeed";
		case "Olx":
			return "OLX";
		case "InternalDatabase":
			return "Internal database";
		default:
			return source;
	}
}

const JobApplicationDetailsPage: React.FC = () => {
	const { id } = useParams<{ id: string }>();
	const changeStatusRef = useRef<ChangeJobApplicationStatusFormCommand>(null);
	const addNoteRef = useRef<AddJobApplicationNoteFormCommand>(null);
	const editRef = useRef<EditApplicantCommand>(null);
	const scheduleRef = useRef<ScheduleInterviewCommand>(null);

	const queryClient = useQueryClient();
	const applicationQuery = useQuery({
		queryKey: ["job-application", id],
		queryFn: ({ signal }) => {
			if (!id) {
				throw new Error("Job application id is required");
			}

			return getJobApplication(id, undefined, signal);
		},
		enabled: Boolean(id),
	});

	if (!id) {
		return (
			<div className="job-application-details">
				<div className="data-details-empty">Application not found.</div>
			</div>
		);
	}

	if (applicationQuery.isLoading) {
		return (
			<div className="job-application-details">
				<div className="data-details-loading">Loading application...</div>
			</div>
		);
	}

	if (applicationQuery.isError || !applicationQuery.data) {
		return (
			<div className="job-application-details">
				<div className="data-details-error">Unable to load application.</div>
			</div>
		);
	}
	const refetch = () => {
		applicationQuery.refetch();
	};

	const application = applicationQuery.data;

	const tags = application?.tags?.length
		? application.tags.map((z) => z.name)
		: ["C#", "Java", "Postgres"];

	return (
		<DataDetails>
			<DetailsHeader
				name={application.applicantFullName}
				detailsAddons={
					<div className="data-details-header-meta">
						<ApplicationBadge status={application.status} />

						<span className="data-details-header-info">{getSourceLabel(application.source)}</span>
					</div>
				}
				onEdit={() => {
					editRef.current?.edit(application.id);
				}}
				extraAdd={
					<DetailsActions
						addNote={() => {
							addNoteRef.current?.addNote(application.id);
						}}
						addTag={() => { }}
						onChangeStatus={() => {
							changeStatusRef.current?.changeStatus(application.id, application.status);
						}}
						scheduleInterview={() => {
							scheduleRef.current?.schedule(application.id);
						}}
					/>
				}
			/>

			<DataDetailsLayout
				main={
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
									<a href={`mailto:${application.applicantEmail}`}>{application.applicantEmail}</a>
								</DetailItem>

								<DetailItem label="Phone">
									<a href={`tel:${application.applicantPhone}`}>{application.applicantPhone}</a>
								</DetailItem>

								<DetailItem label="Candidate ID">{application.candidateId}</DetailItem>

								<DetailItem label="Source">{getSourceLabel(application.source)}</DetailItem>
							</dl>
						</section>
						<section className="data-details-section">
							<div className="data-details-section-header">
								<div>
									<h2>Candidate</h2>
									<p>Candidate profile</p>
								</div>
							</div>

							<dl className="data-details-list">
								<DetailItem label="Candidate">
									{application.candidateInfo?.fullName ?? application.applicantFullName}
								</DetailItem>

								<DetailItem label="Candidate ID">{application.candidateId}</DetailItem>

								<DetailItem label="Latest interview">
									{application.latestInterviewId ? <span>Scheduled</span> : "—"}
								</DetailItem>
							</dl>
						</section>
						<NotesList id={application.id} />
					</>
				}
				sidebar={
					<>
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

								<DetailItem label="Created">{formatDate(application.createdAt)}</DetailItem>

								<DetailItem label="Last modified">{formatDate(application.updatedAt)}</DetailItem>

								<DetailItem label="Modified by">{application.modifiedBy?.fullname}</DetailItem>
							</dl>
						</section>
						<div className="data-content-lists ">
							<DetailsListSection title="Tags" items={tags} className="short-items-section" />
						</div>
					</>
				}
			/>

			<AddJobApplicationNoteDrawer
				ref={addNoteRef}
				onSuccess={() => {
					refetch();
					queryClient.invalidateQueries({
						queryKey: ["job-application-notes", application.id],
					});
				}}
			/>
			<ChangeJobApplicationStatusDrawer ref={changeStatusRef} onSuccess={refetch} />
			<EditApplicantDrawer ref={editRef} onSuccess={refetch} />
			<ScheduletInterviewDrawer ref={scheduleRef} onSuccess={refetch} />

			<ChangeJobApplicationStatusDrawer ref={changeStatusRef} onSuccess={refetch} />
		</DataDetails>
	);
};

export default JobApplicationDetailsPage;
