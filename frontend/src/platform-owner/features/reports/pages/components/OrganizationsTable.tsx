import { Table } from "@heroui/react";
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

			<Table variant="secondary" className="table-container">
				<Table.ScrollContainer>
					<Table.Content aria-label="Organizations">
						<Table.Header>
							<Table.Column isRowHeader>Organization</Table.Column>
							<Table.Column className="table-header-sm">Job posts</Table.Column>
							<Table.Column className="table-header-sm">Applications</Table.Column>
							<Table.Column className="table-header-sm">Interviews</Table.Column>
							<Table.Column className="table-header-sm">Offers</Table.Column>
							<Table.Column className="table-header-sm">Hires</Table.Column>
							<Table.Column className="table-header-sm">Projects live</Table.Column>
							<Table.Column className="table-header-sm">Projects active</Table.Column>
							<Table.Column className="table-header-md">Last activity</Table.Column>
						</Table.Header>
						<Table.Body>
							{organizations.map((organization) => (
								<Table.Row key={organization.organizationId}>
									<Table.Cell>
										<div>{organization.name}</div>
										<div className="report-hint">{organization.slug}</div>
									</Table.Cell>
									<Table.Cell className="table-figure">{organization.jobPostsPublished}</Table.Cell>
									<Table.Cell className="table-figure">{organization.applications}</Table.Cell>
									<Table.Cell className="table-figure">
										{organization.interviewsScheduled}
									</Table.Cell>
									<Table.Cell className="table-figure">{organization.offers}</Table.Cell>
									<Table.Cell className="table-figure">{organization.hires}</Table.Cell>
									<Table.Cell className="table-figure">{organization.projectsWentLive}</Table.Cell>
									<Table.Cell className="table-figure">{organization.projectsActive}</Table.Cell>
									<Table.Cell className="table-figure">
										{organization.lastActivityAt
											? formatDateTime(organization.lastActivityAt)
											: "—"}
									</Table.Cell>
								</Table.Row>
							))}
						</Table.Body>
					</Table.Content>
				</Table.ScrollContainer>
			</Table>
		</section>
	);
}
