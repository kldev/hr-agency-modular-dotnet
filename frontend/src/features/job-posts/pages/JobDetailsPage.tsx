import { useNavigate, useParams } from "@tanstack/react-router";
import { Languages, Pencil, PlusIcon } from "lucide-react";
import { useRef } from "react";
import type { JobPostProjection } from "@/api/models";
import {
	formatSalary,
	getCountryLabel,
	getEmploymentTypeLabel,
	getJobPostStatusLabel,
	getWorkModeLabel,
} from "@/components";
import {
	Button,
	DataDetails,
	DetailsHeader,
	DetailsListSection,
	JobPostsBadge,
} from "@/components/ui";
import { DataDetailsLayout, DetailItem } from "@/components/ui/details/DataDetails";
import {
	type CreateJobApplicationsCommand,
	CreateJobApplicationsDrawer,
} from "@/features/applications/pages/components";
import { formatDateTime } from "@/utlis";
import { useGetJobPost } from "./hooks";

function JobPostOverview({ jobPost }: { jobPost: JobPostProjection }) {
	return (
		<section className="data-details-section">
			<div className="data-details-section-header">
				<div>
					<h2>Overview</h2>
					<p>Job description details</p>
				</div>
			</div>

			<dl className="data-details-list">
				<DetailItem label="Company">{jobPost.company.name}</DetailItem>

				<DetailItem label="Status">
					<span
						className={`data-details-status data-details-status--${jobPost.status.toLowerCase()}`}
					>
						{getJobPostStatusLabel(jobPost.status)}
					</span>
				</DetailItem>

				<DetailItem label="Employment type">
					{getEmploymentTypeLabel(jobPost.employmentType)}
				</DetailItem>

				<DetailItem label="Work mode">{getWorkModeLabel(jobPost.workMode)}</DetailItem>

				<DetailItem label="Location">{jobPost.location || "—"}</DetailItem>

				<DetailItem label="Country">{getCountryLabel(jobPost.countryCode)}</DetailItem>

				<DetailItem label="Salary">
					{formatSalary(jobPost.salaryMin, jobPost.salaryMax, jobPost.currencyCode)}
				</DetailItem>

				<DetailItem label="Recruiter">{jobPost.recruiter.fullname}</DetailItem>
			</dl>
		</section>
	);
}

function JobPostDescription({ jobPost }: { jobPost: JobPostProjection }) {
	return (
		<section className="data-details-section">
			<div className="data-details-section-header">
				<div>
					<h2>Description</h2>
					<p>Position summary and description</p>
				</div>
			</div>

			<div className="data-details-text">
				{jobPost.summary && <p className="data-details-summary">{jobPost.summary}</p>}

				{jobPost.description && <div className="data-details-body">{jobPost.description}</div>}
			</div>
		</section>
	);
}

const JobPostDetailsPage: React.FC = () => {
	const { id } = useParams({ from: "/app/jobs/$id" });
	const navigate = useNavigate();
	const addAppRef = useRef<CreateJobApplicationsCommand>(null);

	/*
	 * The same key the edit wizard and the mutations invalidate - fetching inline left this screen
	 * with its own, never invalidated cache entry, so a save showed stale data on the way back.
	 */
	const jobPostQuery = useGetJobPost(id);

	if (!id) {
		return (
			<div className="data-details-details">
				<div className="data-details-empty">Job post not found.</div>
			</div>
		);
	}

	if (jobPostQuery.isLoading) {
		return (
			<div className="data-details-details">
				<div className="data-details-loading">Loading job post...</div>
			</div>
		);
	}

	if (jobPostQuery.isError || !jobPostQuery.data) {
		return (
			<div className="data-details-details">
				<div className="data-details-error">Unable to load job post.</div>
			</div>
		);
	}

	const jobPost = jobPostQuery.data;

	return (
		<DataDetails>
			<DetailsHeader
				name={jobPost.title}
				detailsAddons={
					<div className="data-details-header-meta">
						<JobPostsBadge status={jobPost.status} />

						<span className="data-details-header-info">{jobPost.company.name}</span>
					</div>
				}
				extraAdd={
					<div className="flex justify-end flex-row gap-2">
						<Button
							variant="ghost"
							title="Add application"
							onClick={() => {
								addAppRef.current?.create(jobPost.id, jobPost.title);
							}}
						>
							<PlusIcon size={15} />
							<span>Add application</span>
						</Button>
						<Button
							variant="ghost"
							title="Copy to new language"
							onClick={() =>
								navigate({
									to: "/app/jobs/add",
									search: { fromJobPostId: jobPost.id, jobDescriptionId: undefined },
								})
							}
						>
							<Languages size={15} />
							<span>Copy to new language</span>
						</Button>
						<Button
							variant="ghost"
							title="Edit data"
							onClick={() =>
								navigate({
									to: "/app/jobs/edit/$id",
									params: { id: jobPost.id },
								})
							}
						>
							<Pencil size={15} />
							<span>Edit</span>
						</Button>
					</div>
				}
			/>

			<DataDetailsLayout
				main={
					<>
						<JobPostDescription jobPost={jobPost} />
						<div className="data-content-lists">
							<DetailsListSection title="Responsibilities" items={jobPost.responsibilities} />

							<DetailsListSection title="Requirements" items={jobPost.requirements} />

							<DetailsListSection
								title="Skills"
								items={jobPost.skills}
								className="short-items-section"
							/>
						</div>
					</>
				}
				sidebar={
					<>
						<JobPostOverview jobPost={jobPost} />

						<section className="data-details-section">
							<div className="data-details-section-header">
								<div>
									<h2>Audit</h2>
									<p>Record information</p>
								</div>
							</div>

							<dl className="data-details-list">
								<DetailItem label="Created">{formatDateTime(jobPost.createdAt)}</DetailItem>

								<DetailItem label="Created by">{jobPost.createdBy.fullname}</DetailItem>

								<DetailItem label="Updated">{formatDateTime(jobPost.updatedAt)}</DetailItem>

								<DetailItem label="Modified by">{jobPost.modifiedBy?.fullname}</DetailItem>
							</dl>
						</section>
					</>
				}
			/>

			<CreateJobApplicationsDrawer ref={addAppRef} onSuccess={() => {}} />
		</DataDetails>
	);
};

export default JobPostDetailsPage;
