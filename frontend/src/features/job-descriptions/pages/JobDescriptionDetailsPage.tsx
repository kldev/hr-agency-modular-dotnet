import { useQuery } from "@tanstack/react-query";
import { useParams } from "react-router-dom";
import "./job-description-details.css";

import { getJobDescription } from "@/api/endpoints";
import type { JobDescriptionProjection } from "@/api/models";
import { DataDetails, DetailsHeader } from "@/components/ui";
import { DetailItem } from "@/components/ui/details/DataDetails";

function formatSalary(
	min: number | string,
	max: number | string,
	currencyCode: string,
) {
	const formatter = new Intl.NumberFormat("pl-PL", {
		minimumFractionDigits: 0,
		maximumFractionDigits: 2,
	});

	return `${formatter.format(Number(min))} – ${formatter.format(Number(max))} ${currencyCode}`;
}

function formatDate(value: string) {
	return new Intl.DateTimeFormat("pl-PL", {
		dateStyle: "medium",
		timeStyle: "short",
	}).format(new Date(value));
}

function getStatusLabel(status: string) {
	switch (status) {
		case "Draft":
			return "Draft";
		case "Active":
			return "Active";
		case "Archived":
			return "Archived";
		default:
			return status;
	}
}

function getEmploymentTypeLabel(type: string) {
	switch (type) {
		case "FullTime":
			return "Full time";
		case "PartTime":
			return "Part time";
		case "Contract":
			return "Contract";
		case "B2B":
			return "B2B";
		case "Internship":
			return "Internship";
		default:
			return type;
	}
}

function getWorkModeLabel(mode: string) {
	switch (mode) {
		case "Remote":
			return "Remote";
		case "Hybrid":
			return "Hybrid";
		case "OnSite":
			return "On-site";
		default:
			return mode;
	}
}

function getCountryLabel(countryCode: string) {
	try {
		return new Intl.DisplayNames(["en"], {
			type: "region",
		}).of(countryCode);
	} catch {
		return countryCode;
	}
}

function ListSection({
	title,
	items,
	className = "",
}: {
	title: string;
	items: string[];
	className?: string;
}) {
	return (
		<section className={`job-description-content-section ${className}`}>
			<header className="job-description-content-header">
				<h2>{title}</h2>
				<span>{items.length}</span>
			</header>

			{items.length === 0 ? (
				<div className="job-description-content-empty">
					No items added.
				</div>
			) : (
				<ul className="job-description-list">
					{items.map((item, index) => (
						<li key={`${item}-${index}`}>{item}</li>
					))}
				</ul>
			)}
		</section>
	);
}

function JobDescriptionOverview({
	jobDescription,
}: {
	jobDescription: JobDescriptionProjection;
}) {
	return (
		<section className="data-details-section">
			<div className="data-details-section-header">
				<div>
					<h2>Overview</h2>
					<p>Job description details</p>
				</div>
			</div>

			<dl className="data-details-list">
				<DetailItem label="Company">
					{jobDescription.company.name}
				</DetailItem>

				<DetailItem label="Status">
					<span
						className={`job-description-status job-description-status--${jobDescription.status.toLowerCase()}`}
					>
						{getStatusLabel(jobDescription.status)}
					</span>
				</DetailItem>

				<DetailItem label="Employment type">
					{getEmploymentTypeLabel(jobDescription.employmentType)}
				</DetailItem>

				<DetailItem label="Work mode">
					{getWorkModeLabel(jobDescription.workMode)}
				</DetailItem>

				<DetailItem label="Location">
					{jobDescription.location || "—"}
				</DetailItem>

				<DetailItem label="Country">
					{getCountryLabel(jobDescription.countryCode)}
				</DetailItem>

				<DetailItem label="Salary">
					{formatSalary(
						jobDescription.salaryMin,
						jobDescription.salaryMax,
						jobDescription.currencyCode,
					)}
				</DetailItem>

				<DetailItem label="Recruiter">
					{jobDescription.recruiter.fullname}
				</DetailItem>
			</dl>
		</section>
	);
}

function JobDescriptionDescription({
	jobDescription,
}: {
	jobDescription: JobDescriptionProjection;
}) {
	return (
		<section className="data-details-section">
			<div className="data-details-section-header">
				<div>
					<h2>Description</h2>
					<p>Position summary and description</p>
				</div>
			</div>

			<div className="job-description-text">
				{jobDescription.summary && (
					<p className="job-description-summary">
						{jobDescription.summary}
					</p>
				)}

				{jobDescription.description && (
					<div className="job-description-body">
						{jobDescription.description}
					</div>
				)}
			</div>
		</section>
	);
}

const JobDescriptionDetailsPage: React.FC = () => {
	const { id } = useParams<{ id: string }>();

	const jobDescriptionQuery = useQuery({
		queryKey: ["job-description", id],
		queryFn: ({ signal }) => {
			if (!id) {
				throw new Error("Job description id is required");
			}

			return getJobDescription(id, undefined, signal);
		},
		enabled: Boolean(id),
	});

	if (!id) {
		return (
			<div className="job-description-details">
				<div className="data-details-empty">
					Job description not found.
				</div>
			</div>
		);
	}

	if (jobDescriptionQuery.isLoading) {
		return (
			<div className="job-description-details">
				<div className="data-details-loading">
					Loading job description...
				</div>
			</div>
		);
	}

	if (jobDescriptionQuery.isError || !jobDescriptionQuery.data) {
		return (
			<div className="job-description-details">
				<div className="data-details-error">
					Unable to load job description.
				</div>
			</div>
		);
	}

	const jobDescription = jobDescriptionQuery.data;

	return (
		<DataDetails>
			<DetailsHeader
				name={jobDescription.title}
				detailsAddons={
					<div className="job-description-header-meta">
						<span
							className={`job-description-status job-description-status--${jobDescription.status.toLowerCase()}`}
						>
							{getStatusLabel(jobDescription.status)}
						</span>

						<span className="job-description-company">
							{jobDescription.company.name}
						</span>
					</div>
				}
				onEdit={() => {
					// open edit drawer
				}}
			/>

			<div className="job-description-layout">
				<div className="job-description-main">
					<JobDescriptionDescription jobDescription={jobDescription} />

					<div className="job-description-lists">
						<ListSection
							title="Responsibilities"
							items={jobDescription.responsibilities}
						/>

						<ListSection
							title="Requirements"
							items={jobDescription.requirements}
						/>

						<ListSection
							title="Skills"
							items={jobDescription.skills}
							className="job-description-skills-section"
						/>
					</div>
				</div>

				<aside className="job-description-sidebar">
					<JobDescriptionOverview
						jobDescription={jobDescription}
					/>

					<section className="data-details-section">
						<div className="data-details-section-header">
							<div>
								<h2>Audit</h2>
								<p>Record information</p>
							</div>
						</div>

						<dl className="data-details-list">
							<DetailItem label="Created">
								{formatDate(jobDescription.createdAt)}
							</DetailItem>

							<DetailItem label="Created by">
								{jobDescription.createdBy.fullname}
							</DetailItem>

							<DetailItem label="Updated">
								{formatDate(jobDescription.updatedAt)}
							</DetailItem>

							<DetailItem label="Modified by">
								{jobDescription.modifiedBy?.fullname}
							</DetailItem>
						</dl>
					</section>
				</aside>
			</div>
		</DataDetails>
	);
}

export default JobDescriptionDetailsPage