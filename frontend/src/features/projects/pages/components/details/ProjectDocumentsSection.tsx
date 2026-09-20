import { Download, Paperclip, Trash2 } from "lucide-react";
import { useState } from "react";
import { toast } from "sonner";
import type { ProjectDocument, ProjectProjection } from "@/api/models";
import { Button, ConfirmDialog, DetailOverviewHeader } from "@/components/ui";
import { formatDate } from "@/utlis/dateUtils";
import { documentCategories } from "../../../types";
import { formatFileSize, useRemoveProjectDocument } from "../../hooks";

interface ProjectDocumentsSectionProps {
	project: ProjectProjection;
	onAttach: () => void;
	onRefresh: () => void;
}

export function ProjectDocumentsSection({
	project,
	onAttach,
	onRefresh,
}: ProjectDocumentsSectionProps) {
	const [removing, setRemoving] = useState<ProjectDocument | null>(null);

	const { mutation } = useRemoveProjectDocument({
		onSuccess: () => {
			toast.success("Document removed");
			setRemoving(null);
			onRefresh();
		},
	});

	return (
		<div className="data-overview">
			<DetailOverviewHeader
				title="Documents"
				description="Contracts, permits and everything else worth keeping with the project."
			/>

			{project.documents.length === 0 ? null : (
				<table className="table">
					<thead>
						<tr>
							<th>Category</th>
							<th>Name</th>
							<th>Document date</th>
							<th>Valid until</th>
							<th>Size</th>
							<th />
						</tr>
					</thead>

					<tbody>
						{project.documents.map((document) => (
							<tr key={document.documentId}>
								<td>{documentCategories[document.category]}</td>
								<td className="table-cell-truncate" title={document.fileName}>
									{document.fileName}
								</td>
								<td>{formatDate(document.documentDate)}</td>
								<td>{document.validUntil ? formatDate(document.validUntil) : "—"}</td>
								<td>{formatFileSize(Number(document.size))}</td>
								<td>
									<div className="flex gap-2 justify-end">
										{/*
										 * A plain link: the proxy route attaches the token on the server and the API
										 * streams the bytes from the file service. The browser never sees a storage
										 * key, and there is no such thing in the generated types to leak.
										 */}
										<a
											className="action-button"
											href={`/api/projects/${project.id}/documents/${document.documentId}/content`}
											title="Download"
											aria-label="Download"
										>
											<Download size={15} />
										</a>

										<button
											type="button"
											className="action-button"
											title="Remove"
											aria-label="Remove"
											onClick={() => setRemoving(document)}
										>
											<Trash2 size={15} />
										</button>
									</div>
								</td>
							</tr>
						))}
					</tbody>
				</table>
			)}

			<div className="project-section-body">
				{project.documents.length === 0 ? (
					<p className="project-role-empty">Nothing attached yet.</p>
				) : null}

				<Button variant="ghost" icon={<Paperclip size={15} />} onClick={onAttach}>
					Attach document
				</Button>
			</div>

			<ConfirmDialog
				open={Boolean(removing)}
				title="Remove this document?"
				description={
					removing
						? `"${removing.fileName}" will be removed from the project and from storage. A document that proves a compliance item cannot be removed.`
						: ""
				}
				confirmLabel="Remove"
				loading={mutation.isPending}
				onConfirm={() => {
					if (!removing) return;

					mutation.mutate({ projectId: project.id, documentId: removing.documentId });
				}}
				onClose={() => setRemoving(null)}
			/>
		</div>
	);
}
