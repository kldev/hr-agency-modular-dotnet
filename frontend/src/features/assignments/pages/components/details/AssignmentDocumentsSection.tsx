import { FileText } from "lucide-react";
import type { AssignmentProjection } from "@/api/models";
import { DetailOverviewHeader, EmptyState } from "@/components/ui";
import { formatFileSize } from "@/utlis";
import { formatDate } from "@/utlis/dateUtils";
import { assignmentDocumentCategories } from "../../../types";

interface AssignmentDocumentsSectionProps {
	assignment: AssignmentProjection;
	onAttach?: () => void;
}

/** This posting's own paperwork, as opposed to the person's: the contract with this entity, the
 *  A1 for this period, the host country notification. */
export function AssignmentDocumentsSection({
	assignment,
	onAttach,
}: AssignmentDocumentsSectionProps) {
	const documents = assignment.documents ?? [];

	return (
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
							<th>Issued</th>
							<th>Valid until</th>
							<th>Size</th>
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

								<td className="table-number">{formatDate(document.documentDate)}</td>

								<td className="table-number">
									{document.validUntil ? formatDate(document.validUntil) : "—"}
								</td>

								<td className="table-number">{formatFileSize(Number(document.size))}</td>
							</tr>
						))}
					</tbody>
				</table>
			)}
		</div>
	);
}
