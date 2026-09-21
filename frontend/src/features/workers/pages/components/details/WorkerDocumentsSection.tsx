import { FileText } from "lucide-react";
import type { WorkerProjection } from "@/api/models";
import { DetailOverviewHeader, EmptyState } from "@/components/ui";
import { formatFileSize } from "@/utlis";
import { formatDate } from "@/utlis/dateUtils";
import { workerDocumentCategories } from "../../../types";

interface WorkerDocumentsSectionProps {
	worker: WorkerProjection;
	onAttach?: () => void;
}

export function WorkerDocumentsSection({ worker, onAttach }: WorkerDocumentsSectionProps) {
	const documents = worker.documents ?? [];

	return (
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
								<td>{workerDocumentCategories[document.category]}</td>

								<td className="table-cell-truncate" title={document.fileName}>
									{/* The API streams from the file service; the browser never sees a storage
									    key, and orval's types do not even carry one. */}
									<a href={`/api/workers/${worker.id}/documents/${document.documentId}/content`}>
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
