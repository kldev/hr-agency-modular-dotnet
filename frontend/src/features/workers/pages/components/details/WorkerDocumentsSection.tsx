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
									<td>{workerDocumentCategories[document.category]}</td>

									<td className="table-cell-truncate" title={document.fileName}>
										{/* The API streams from the file service; the browser never sees a storage
									    key, and orval's types do not even carry one. */}
										<a href={`/api/workers/${worker.id}/documents/${document.documentId}/content`}>
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
