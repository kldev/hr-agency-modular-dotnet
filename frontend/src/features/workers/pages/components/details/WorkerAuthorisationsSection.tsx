import { Table } from "@heroui/react";
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
					<Table variant="secondary" className="table-container">
						<Table.ScrollContainer>
							<Table.Content aria-label="Work authorisations">
								<Table.Header>
									<Table.Column isRowHeader>Kind</Table.Column>
									<Table.Column>Country</Table.Column>
									<Table.Column>Number</Table.Column>
									<Table.Column className="table-header-md">Valid</Table.Column>
									<Table.Column aria-label="Actions" />
								</Table.Header>

								<Table.Body>
									{authorisations.map((authorisation) => (
										<Table.Row key={authorisation.authorisationId}>
											<Table.Cell>{workAuthorisationKinds[authorisation.kind]}</Table.Cell>
											<Table.Cell>{getCountryLabel(authorisation.country)}</Table.Cell>
											<Table.Cell>{authorisation.number}</Table.Cell>
											<Table.Cell className="table-figure">
												{formatPeriod(authorisation.validFrom, authorisation.validUntil)}
											</Table.Cell>

											<Table.Cell>
												<div className="flex justify-end">
													<Button
														variant="ghost"
														icon={<Trash2 size={15} />}
														aria-label="Remove"
														title="Remove"
														onPress={() => setRemoving(authorisation)}
													/>
												</div>
											</Table.Cell>
										</Table.Row>
									))}
								</Table.Body>
							</Table.Content>
						</Table.ScrollContainer>
					</Table>
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
