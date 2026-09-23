import type { OrganizationActivity } from "#/api/models";
import { formatDateTime } from "#/utlis/dateUtils";
import { DetailOverviewHeader } from "@/components/ui";

export function OrganizationsTable({ organizations }: { organizations: OrganizationActivity[] }) {
	return (
		<section className="data-details-section">
			<DetailOverviewHeader
				title="Organizations"
				description="Activity in the period, most recently active first. Projects active is today's count."
			/>

			<table className="table">
				<thead>
					<tr>
						<th>Organization</th>
						<th className="table-header-sm">Job posts</th>
						<th className="table-header-sm">Applications</th>
						<th className="table-header-sm">Interviews</th>
						<th className="table-header-sm">Offers</th>
						<th className="table-header-sm">Hires</th>
						<th className="table-header-sm">Projects live</th>
						<th className="table-header-sm">Projects active</th>
						<th className="table-header-md">Last activity</th>
					</tr>
				</thead>
				<tbody>
					{organizations.map((organization) => (
						<tr key={organization.organizationId}>
							<td>
								<div>{organization.name}</div>
								<div className="report-hint">{organization.slug}</div>
							</td>
							<td className="table-figure">{organization.jobPostsPublished}</td>
							<td className="table-figure">{organization.applications}</td>
							<td className="table-figure">{organization.interviewsScheduled}</td>
							<td className="table-figure">{organization.offers}</td>
							<td className="table-figure">{organization.hires}</td>
							<td className="table-figure">{organization.projectsWentLive}</td>
							<td className="table-figure">{organization.projectsActive}</td>
							<td className="table-figure">
								{organization.lastActivityAt ? formatDateTime(organization.lastActivityAt) : "—"}
							</td>
						</tr>
					))}
				</tbody>
			</table>
		</section>
	);
}
