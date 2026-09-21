import { ShieldCheck } from "lucide-react";
import type { WorkerProjection } from "@/api/models";
import { getCountryLabel } from "@/components/labels";
import { DetailOverviewHeader, EmptyState } from "@/components/ui";
import { formatPeriod } from "@/utlis/formatRecord";
import { workAuthorisationKinds } from "../../../types";

interface WorkerAuthorisationsSectionProps {
	worker: WorkerProjection;
	onRecord?: () => void;
}

/**
 * Only rendered for somebody who needs legalisation. The backend refuses to record a permit for an
 * EEA national outright — "This person holds free movement rights and needs no permission to work"
 * — so an empty section with a button that always returns 400 would be an invitation to a mistake.
 */
export function WorkerAuthorisationsSection({
	worker,
	onRecord,
}: WorkerAuthorisationsSectionProps) {
	const authorisations = worker.authorisations ?? [];

	return (
		<div className="data-overview">
			<DetailOverviewHeader
				title="Permissions to work"
				description="What lets this person work here, and until when."
				onAdd={onRecord}
			/>

			{authorisations.length === 0 ? (
				<EmptyState
					title="Nothing recorded yet"
					description="A residence title and a work permit are what the legalisation stage is for."
				>
					<ShieldCheck size={24} />
				</EmptyState>
			) : (
				<table className="table">
					<thead>
						<tr>
							<th>Kind</th>
							<th>Country</th>
							<th>Number</th>
							<th>Valid</th>
						</tr>
					</thead>

					<tbody>
						{authorisations.map((authorisation) => (
							<tr key={authorisation.authorisationId}>
								<td>{workAuthorisationKinds[authorisation.kind]}</td>
								<td>{getCountryLabel(authorisation.country)}</td>
								<td>{authorisation.number}</td>
								<td className="table-number">
									{formatPeriod(authorisation.validFrom, authorisation.validUntil)}
								</td>
							</tr>
						))}
					</tbody>
				</table>
			)}
		</div>
	);
}
