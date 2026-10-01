import { Table } from "@heroui/react";
import { Download, Paperclip, Trash2 } from "lucide-react";
import { useState } from "react";
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
				<Table variant="secondary" className="table-container">
					<Table.ScrollContainer>
						<Table.Content aria-label="Documents">
							<Table.Header>
								<Table.Column isRowHeader>Category</Table.Column>
								<Table.Column>Name</Table.Column>
								<Table.Column>Document date</Table.Column>
								<Table.Column>Valid until</Table.Column>
								<Table.Column>Size</Table.Column>
								<Table.Column aria-label="Actions" />
							</Table.Header>

							<Table.Body>
								{project.documents.map((document) => (
									<Table.Row key={document.documentId}>
										<Table.Cell>{documentCategories[document.category]}</Table.Cell>
										<Table.Cell className="table-cell-truncate">
											<span title={document.fileName}>{document.fileName}</span>
										</Table.Cell>
										<Table.Cell>{formatDate(document.documentDate)}</Table.Cell>
										<Table.Cell>
											{document.validUntil ? formatDate(document.validUntil) : "—"}
										</Table.Cell>
										<Table.Cell>{formatFileSize(Number(document.size))}</Table.Cell>
										<Table.Cell>
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
										</Table.Cell>
									</Table.Row>
								))}
							</Table.Body>
						</Table.Content>
					</Table.ScrollContainer>
				</Table>
			)}

			<div className="project-section-body">
				{project.documents.length === 0 ? (
					<p className="project-role-empty">Nothing attached yet.</p>
				) : null}

				<Button variant="ghost" icon={<Paperclip size={15} />} onPress={onAttach}>
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
