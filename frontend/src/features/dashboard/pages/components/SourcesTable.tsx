import { Table } from "@heroui/react";
import type { CandidateSource, SourceCount } from "#/api/models";
import { applicationSources } from "#/features/applications/types";
import { DetailOverviewHeader } from "@/components/ui";

interface SourcesTableProps {
	sources: SourceCount[];
	total: number;
}

export function SourcesTable({ sources, total }: SourcesTableProps) {
	return (
		<section className="data-details-section">
			<DetailOverviewHeader
				title="Where applications come from"
				description="Applications received in the period, by the channel the candidate came through."
			/>

			{sources.length === 0 ? (
				<p className="report-section-body report-hint">No applications in this period.</p>
			) : (
				<Table variant="secondary" className="table-container">
					<Table.ScrollContainer>
						<Table.Content aria-label="Candidate sources">
							<Table.Header>
								<Table.Column isRowHeader>Source</Table.Column>
								<Table.Column className="table-header-sm">Applications</Table.Column>
								<Table.Column className="table-header-sm">Share</Table.Column>
							</Table.Header>
							<Table.Body>
								{sources.map((source) => (
									<Table.Row key={source.source}>
										<Table.Cell>
											{applicationSources[source.source as CandidateSource] ?? source.source}
										</Table.Cell>
										<Table.Cell className="table-figure">{source.applications}</Table.Cell>
										<Table.Cell className="table-figure">
											{total === 0
												? "—"
												: `${Math.round((Number(source.applications) / total) * 100)}%`}
										</Table.Cell>
									</Table.Row>
								))}
							</Table.Body>
						</Table.Content>
					</Table.ScrollContainer>
				</Table>
			)}
		</section>
	);
}
