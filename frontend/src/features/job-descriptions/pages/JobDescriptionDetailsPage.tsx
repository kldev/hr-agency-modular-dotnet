import type { JobDescriptionProjection } from "@/api/models";
import {
	formatSalary,
	getCountryLabel,
	getEmploymentTypeLabel,
	getWorkModeLabel,
} from "@/components";
import {
	DataDetails,
	DetailsHeader,
	DetailsListSection,
	JobDescriptionBadge,
} from "@/components/ui";
import { DataDetailsLayout, DetailItem } from "@/components/ui/details/DataDetails";
import { formatDateTime } from "@/utlis";

function JobDescriptionOverview({ jobDescription }: { jobDescription: JobDescriptionProjection }) {
	return (
		<section className="data-details-section">
			<div className="data-details-section-header">
				<div>
					<h2>Overview</h2>
					<p>Job description details</p>
				</div>
			</div>

			<dl className="data-details-list">
				<DetailItem label="Company">{jobDescription.company.name}</DetailItem>

				<DetailItem label="Status">
					<JobDescriptionBadge status={jobDescription.status} />
				</DetailItem>

				<DetailItem label="Employment type">
					{getEmploymentTypeLabel(jobDescription.employmentType)}
				</DetailItem>

				<DetailItem label="Work mode">{getWorkModeLabel(jobDescription.workMode)}</DetailItem>

				<DetailItem label="Location">{jobDescription.location || "—"}</DetailItem>

				<DetailItem label="Country">{getCountryLabel(jobDescription.countryCode)}</DetailItem>

				<DetailItem label="Salary">
					{formatSalary(
						jobDescription.salaryMin,
						jobDescription.salaryMax,
						jobDescription.currencyCode,
					)}
				</DetailItem>

				<DetailItem label="Recruiter">{jobDescription.recruiter.fullname}</DetailItem>
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

			<div className="data-details-text">
				{jobDescription.summary && <p className="data-details-summary">{jobDescription.summary}</p>}

				{jobDescription.description && (
					<div className="data-details-body">{jobDescription.description}</div>
				)}
			</div>
		</section>
	);
}

const JobDescriptionDetailsPage: React.FC<{ jobDescription: JobDescriptionProjection }> = ({
	jobDescription,
}) => {
	return (
		<DataDetails>
			<DetailsHeader
				name={jobDescription.title}
				detailsAddons={
					<div className="data-details-header-meta">
						<JobDescriptionBadge status={jobDescription.status} />

						<span className="data-details-header-info">{jobDescription.company.name}</span>
					</div>
				}
				onEdit={() => {
					// open edit drawer
				}}
			/>

			<DataDetailsLayout
				main={
					<>
						<JobDescriptionDescription jobDescription={jobDescription} />

						<div className="data-content-lists ">
							<DetailsListSection
								title="Responsibilities"
								items={jobDescription.responsibilities}
							/>

							<DetailsListSection title="Requirements" items={jobDescription.requirements} />

							<DetailsListSection
								title="Skills"
								items={jobDescription.skills}
								className="short-items-section"
							/>
						</div>
					</>
				}
				sidebar={
					<>
						<JobDescriptionOverview jobDescription={jobDescription} />

						<section className="data-details-section">
							<div className="data-details-section-header">
								<div>
									<h2>Audit</h2>
									<p>Record information</p>
								</div>
							</div>

							<dl className="data-details-list">
								<DetailItem label="Created">{formatDateTime(jobDescription.createdAt)}</DetailItem>

								<DetailItem label="Created by">{jobDescription.createdBy.fullname}</DetailItem>

								<DetailItem label="Updated">{formatDateTime(jobDescription.updatedAt)}</DetailItem>

								<DetailItem label="Modified by">{jobDescription.modifiedBy?.fullname}</DetailItem>
							</dl>
						</section>
					</>
				}
			/>
		</DataDetails>
	);
};

export default JobDescriptionDetailsPage;
