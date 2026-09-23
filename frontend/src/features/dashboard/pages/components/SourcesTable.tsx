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
				<table className="table">
					<thead>
						<tr>
							<th>Source</th>
							<th className="table-header-sm">Applications</th>
							<th className="table-header-sm">Share</th>
						</tr>
					</thead>
					<tbody>
						{sources.map((source) => (
							<tr key={source.source}>
								<td>{applicationSources[source.source as CandidateSource] ?? source.source}</td>
								<td className="table-figure">{source.applications}</td>
								<td className="table-figure">
									{total === 0
										? "—"
										: `${Math.round((Number(source.applications) / total) * 100)}%`}
								</td>
							</tr>
						))}
					</tbody>
				</table>
			)}
		</section>
	);
}
