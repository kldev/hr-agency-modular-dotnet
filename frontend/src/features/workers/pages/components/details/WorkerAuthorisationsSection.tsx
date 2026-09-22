import { ShieldCheck, Trash2 } from "lucide-react";
import { useState } from "react";
import type { WorkAuthorisation, WorkerProjection } from "@/api/models";
import { getCountryLabel } from "@/components/labels";
import { Button, ConfirmDialog, DetailOverviewHeader, EmptyState } from "@/components/ui";
import { formatPeriod } from "@/utlis/formatRecord";
import { workAuthorisationKinds } from "../../../types";
import { useRemoveWorkAuthorisation } from "../../hooks";

interface WorkerAuthorisationsSectionProps {
	worker: WorkerProjection;
	onRecord?: () => void;
	onRefresh?: () => void;
}

/**
 * Only rendered for somebody who needs legalisation. The backend refuses to record a permit for an
 * EEA national outright — "This person holds free movement rights and needs no permission to work"
 * — so an empty section with a button that always returns 400 would be an invitation to a mistake.
 */
export function WorkerAuthorisationsSection({
	worker,
	onRecord,
	onRefresh,
}: WorkerAuthorisationsSectionProps) {
	const authorisations = worker.authorisations ?? [];
	const [removing, setRemoving] = useState<WorkAuthorisation | null>(null);

	const { mutation } = useRemoveWorkAuthorisation({
		onSuccess: () => {
			setRemoving(null);
			onRefresh?.();
		},
	});

	return (
		<>
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
								<th className="table-header-md">Valid</th>
								<th />
							</tr>
						</thead>

						<tbody>
							{authorisations.map((authorisation) => (
								<tr key={authorisation.authorisationId}>
									<td>{workAuthorisationKinds[authorisation.kind]}</td>
									<td>{getCountryLabel(authorisation.country)}</td>
									<td>{authorisation.number}</td>
									<td className="table-figure">
										{formatPeriod(authorisation.validFrom, authorisation.validUntil)}
									</td>

									<td>
										<div className="flex justify-end">
											<Button
												variant="ghost"
												icon={<Trash2 size={15} />}
												aria-label="Remove"
												title="Remove"
												onClick={() => setRemoving(authorisation)}
											/>
										</div>
									</td>
								</tr>
							))}
						</tbody>
					</table>
				)}
			</div>

			<ConfirmDialog
				open={Boolean(removing)}
				title="Remove this permission?"
				description="The record goes; the document behind it, if any, stays on file."
				confirmLabel="Remove"
				onConfirm={() =>
					mutation.mutate({
						workerId: worker.id ?? "",
						authorisationId: removing?.authorisationId ?? "",
					})
				}
				onClose={() => setRemoving(null)}
			/>
		</>
	);
}
