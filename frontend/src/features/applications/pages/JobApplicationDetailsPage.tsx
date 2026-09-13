import { useQuery } from "@tanstack/react-query";
import { useParams } from "react-router-dom";

import { getJobApplication } from "@/api/endpoints";
import { DataDetails, DetailsHeader } from "@/components/ui";
import { DetailItem } from "@/components/ui/details/DataDetails";

import "./job-application-details.css";
import { useRef } from "react";
import type { CandidateSource } from "@/api/models";
import { ChangeJobApplicationStatusDrawer, type ChangeJobApplicationStatusFormCommand } from "./components";

function formatDate(value?: string | null) {
	if (!value) return "—";

	return new Intl.DateTimeFormat("pl-PL", {
		dateStyle: "medium",
		timeStyle: "short",
	}).format(new Date(value));
}

function getStatusLabel(status: string) {
	switch (status) {
		case "Applied":
			return "Applied";
		case "Screening":
			return "Screening";
		case "Assessment":
			return "Assessment";
		case "Interview":
			return "Interview";
		case "Offer":
			return "Offer";
		case "Hired":
			return "Hired";
		case "Rejected":
			return "Rejected";
		case "Withdrawn":
			return "Withdrawn";
		case "Reactivated":
			return "Reactivated";
		default:
			return status;
	}
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
	const statusRef = useRef<ChangeJobApplicationStatusFormCommand>(null);

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

	const application = applicationQuery.data;

	return (
		<DataDetails>
			<DetailsHeader
				name={application.applicantFullName}
				detailsAddons={
					<div className="job-application-header-meta">
						<span
							className={`job-application-status job-application-status--${application.status.toLowerCase()}`}
						>
							{getStatusLabel(application.status)}
						</span>

						<span className="job-application-source">{getSourceLabel(application.source)}</span>
					</div>
				}
				onEdit={() => {
					statusRef.current?.changeStatus(application.id, application.status);
				}}
			/>

			<div className="job-application-details-grid">
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
							<h2>Application</h2>
							<p>Application and recruitment details</p>
						</div>
					</div>

					<dl className="data-details-list">
						<DetailItem label="Job post">{application.jobPostTitle}</DetailItem>

						<DetailItem label="Company">{application.company.name}</DetailItem>

						<DetailItem label="Status">
							<span
								className={`job-application-status job-application-status--${application.status.toLowerCase()}`}
							>
								{getStatusLabel(application.status)}
							</span>
						</DetailItem>

						<DetailItem label="Created">{formatDate(application.createdAt)}</DetailItem>

						<DetailItem label="Last modified">{formatDate(application.updatedAt)}</DetailItem>

						<DetailItem label="Modified by">{application.modifiedBy?.fullname}</DetailItem>
					</dl>
				</section>

				<section className="data-details-section">
					<div className="data-details-section-header">
						<div>
							<h2>Tags</h2>
							<p>{application.tags.length} tags</p>
						</div>
					</div>

					<div className="job-application-tags">
						{application.tags.length === 0 ? (
							<span className="job-application-empty">No tags assigned.</span>
						) : (
							application.tags.map((tag) => (
								<span className="job-application-tag" key={tag.id}>
									{tag.name}
								</span>
							))
						)}
					</div>
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
			</div>
			<ChangeJobApplicationStatusDrawer ref={statusRef} onSuccess={() => { applicationQuery.refetch() }} />
		</DataDetails>
	);
};

export default JobApplicationDetailsPage;
