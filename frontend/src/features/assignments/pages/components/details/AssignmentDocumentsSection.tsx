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
					<table className="table">
						<thead>
							<tr>
								<th>Category</th>
								<th>File</th>
								<th className="table-header-sm">Issued</th>
								<th className="table-header-sm">Valid until</th>
								<th className="table-header-ssm">Size</th>
								<th />
							</tr>
						</thead>

						<tbody>
							{documents.map((document) => (
								<tr key={document.documentId}>
									<td>{assignmentDocumentCategories[document.category]}</td>

									<td className="table-cell-truncate" title={document.fileName}>
										<a
											href={`/api/assignments/${assignment.id}/documents/${document.documentId}/content`}
										>
											{document.fileName}
										</a>
									</td>

									<td className="table-figure">{formatDate(document.documentDate)}</td>

									<td className="table-figure">
										{document.validUntil ? formatDate(document.validUntil) : "—"}
									</td>

									<td className="table-figure">{formatFileSize(Number(document.size))}</td>

									<td>
										<div className="flex justify-end">
											<Button
												variant="ghost"
												icon={<Trash2 size={15} />}
												aria-label="Remove"
												title="Remove"
												onClick={() => setRemoving(document)}
											/>
										</div>
									</td>
								</tr>
							))}
						</tbody>
					</table>
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
