import { Table } from "@heroui/react";
import { FileText, Trash2 } from "lucide-react";
import { useState } from "react";
import type { AssignmentDocument, AssignmentProjection } from "@/api/models";
import { Button, ConfirmDialog, DetailOverviewHeader, EmptyState } from "@/components/ui";
import { formatFileSize } from "@/utlis";
import { formatDate } from "@/utlis/dateUtils";
import { assignmentDocumentCategories } from "../../../types";
import { useRemoveAssignmentDocument } from "../../hooks";

interface AssignmentDocumentsSectionProps {
	assignment: AssignmentProjection;
	onAttach?: () => void;
	onRefresh?: () => void;
}

/** This posting's own paperwork, as opposed to the person's: the contract with this entity, the
 *  A1 for this period, the host country notification. */
export function AssignmentDocumentsSection({
	assignment,
	onAttach,
	onRefresh,
}: AssignmentDocumentsSectionProps) {
	const documents = assignment.documents ?? [];
	const [removing, setRemoving] = useState<AssignmentDocument | null>(null);

	const { mutation } = useRemoveAssignmentDocument({
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
					description="Issued for this posting and this period. A new posting needs its own."
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
											<Table.Cell>{assignmentDocumentCategories[document.category]}</Table.Cell>

											<Table.Cell className="table-cell-truncate">
												<span title={document.fileName}>
													<a
														href={`/api/assignments/${assignment.id}/documents/${document.documentId}/content`}
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

			{/* A document recorded as proof under a compliance item cannot go: the backend refuses it by
			    name, and that refusal lands in the toast below. */}
			<ConfirmDialog
				open={Boolean(removing)}
				title="Remove this document?"
				description={`${removing?.fileName ?? ""} will be deleted from the file service as well.`}
				confirmLabel="Remove"
				onConfirm={() =>
					mutation.mutate({
						assignmentId: assignment.id,
						documentId: removing?.documentId ?? "",
					})
				}
				onClose={() => setRemoving(null)}
			/>
		</>
	);
}
