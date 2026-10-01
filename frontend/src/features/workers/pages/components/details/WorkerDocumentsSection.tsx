import { Table } from "@heroui/react";
import { FileText, Trash2 } from "lucide-react";
import { useState } from "react";
import type { WorkerDocument, WorkerProjection } from "@/api/models";
import { Button, ConfirmDialog, DetailOverviewHeader, EmptyState } from "@/components/ui";
import { formatFileSize } from "@/utlis";
import { formatDate } from "@/utlis/dateUtils";
import { workerDocumentCategories } from "../../../types";
import { useRemoveWorkerDocument } from "../../hooks";

interface WorkerDocumentsSectionProps {
	worker: WorkerProjection;
	onAttach?: () => void;
	onRefresh?: () => void;
}

export function WorkerDocumentsSection({
	worker,
	onAttach,
	onRefresh,
}: WorkerDocumentsSectionProps) {
	const documents = worker.documents ?? [];
	const [removing, setRemoving] = useState<WorkerDocument | null>(null);

	const { mutation } = useRemoveWorkerDocument({
		onSuccess: () => {
			setRemoving(null);
			onRefresh?.();
		},
	});

	return (
		<>
			<div className="data-overview">
				<DetailOverviewHeader
					title="Documents"
					description="The person's own paperwork: it travels with them to every project."
					onAdd={onAttach}
				/>

				{documents.length === 0 ? (
					<EmptyState title="No documents yet">
						<FileText size={24} />
					</EmptyState>
				) : (
					<Table variant="secondary" className="table-container">
						<Table.ScrollContainer>
							<Table.Content aria-label="Documents">
								<Table.Header>
									<Table.Column isRowHeader>Category</Table.Column>
									<Table.Column>File</Table.Column>
									<Table.Column className="table-header-sm">Issued</Table.Column>
									<Table.Column className="table-header-sm">Valid until</Table.Column>
									<Table.Column className="table-header-ssm">Size</Table.Column>
									<Table.Column aria-label="Actions" />
								</Table.Header>

								<Table.Body>
									{documents.map((document) => (
										<Table.Row key={document.documentId}>
											<Table.Cell>{workerDocumentCategories[document.category]}</Table.Cell>

											<Table.Cell className="table-cell-truncate">
												<span title={document.fileName}>
													{/* The API streams from the file service; the browser never sees a storage
									    key, and orval's types do not even carry one. */}
													<a
														href={`/api/workers/${worker.id}/documents/${document.documentId}/content`}
													>
														{document.fileName}
													</a>
												</span>
											</Table.Cell>

											<Table.Cell className="table-figure">
												{formatDate(document.documentDate)}
											</Table.Cell>

											<Table.Cell className="table-figure">
												{document.validUntil ? formatDate(document.validUntil) : "—"}
											</Table.Cell>

											<Table.Cell className="table-figure">
												{formatFileSize(Number(document.size))}
											</Table.Cell>

											<Table.Cell>
												<div className="flex justify-end">
													<Button
														variant="ghost"
														icon={<Trash2 size={15} />}
														aria-label="Remove"
														title="Remove"
														onPress={() => setRemoving(document)}
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

			{/* A document that is the proof behind a recorded permission cannot go: the backend refuses
		    it by name, and that refusal lands in the toast below. */}
			<ConfirmDialog
				open={Boolean(removing)}
				title="Remove this document?"
				description={`${removing?.fileName ?? ""} will be deleted from the file service as well.`}
				confirmLabel="Remove"
				onConfirm={() =>
					mutation.mutate({ workerId: worker.id ?? "", documentId: removing?.documentId ?? "" })
				}
				onClose={() => setRemoving(null)}
			/>
		</>
	);
}
