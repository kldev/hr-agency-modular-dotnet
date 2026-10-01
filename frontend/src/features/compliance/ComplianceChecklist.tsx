import { Table } from "@heroui/react";
import { ShieldCheck } from "lucide-react";
import type { ComplianceRequirementView } from "@/api/models";
import { Button, ComplianceStatusBadge, DetailOverviewHeader, EmptyState } from "@/components/ui";
import { formatDate } from "@/utlis/dateUtils";
import { complianceRequirements } from "./types";
import "./compliance.css";

interface ComplianceChecklistProps {
	description: string;
	views: ComplianceRequirementView[] | undefined;
	isLoading: boolean;
	isError: boolean;
	emptyTitle: string;
	emptyDescription: string;
	/** Resolves the proof document to a file name; the owning aggregate holds the documents. */
	documentName: (documentId: string | null) => string;
	onRecord: (view: ComplianceRequirementView) => void;
}

/**
 * The catalogue, not the recorded items: a requirement nobody has touched yet exists only in the
 * catalogue, and those are exactly the ones worth seeing. The API already merges the two, so a
 * row without an item is simply "not started".
 *
 * Shared by projects and assignments because it is literally the same table over the same shape —
 * only the command behind the button differs, and that arrives as `onRecord`.
 */
export function ComplianceChecklist({
	description,
	views,
	isLoading,
	isError,
	emptyTitle,
	emptyDescription,
	documentName,
	onRecord,
}: ComplianceChecklistProps) {
	return (
		<div className="data-overview">
			<DetailOverviewHeader title="Compliance" description={description} />

			{isLoading ? (
				<div className="compliance-section-body">
					<p className="compliance-note">Loading…</p>
				</div>
			) : null}

			{isError ? (
				<div className="compliance-section-body">
					<p className="form-error">The compliance catalogue could not be loaded.</p>
				</div>
			) : null}

			{views && views.length === 0 ? (
				<EmptyState title={emptyTitle} description={emptyDescription}>
					<ShieldCheck size={24} />
				</EmptyState>
			) : null}

			{views && views.length > 0 ? (
				<Table variant="secondary" className="table-container">
					<Table.ScrollContainer>
						<Table.Content aria-label="Compliance">
							<Table.Header>
								<Table.Column isRowHeader>Requirement</Table.Column>
								<Table.Column>Status</Table.Column>
								<Table.Column>Reference</Table.Column>
								<Table.Column>Valid</Table.Column>
								<Table.Column>Document</Table.Column>
								<Table.Column aria-label="Actions" />
							</Table.Header>

							<Table.Body>
								{views.map((view) => (
									<Table.Row key={view.requirement}>
										{/* The legal names are long by nature - "Posting beyond 12 months notified
								    (§ 13b AEntG)" - so the cell truncates and the title carries the rest. */}
										<Table.Cell className="table-cell-truncate">
											<span title={complianceRequirements[view.requirement]}>
												{complianceRequirements[view.requirement]}
											</span>
										</Table.Cell>

										<Table.Cell>
											<ComplianceStatusBadge status={view.item?.status ?? "NotStarted"} />
										</Table.Cell>

										<Table.Cell>{view.item?.referenceNumber ?? "—"}</Table.Cell>

										<Table.Cell>
											{view.item?.validFrom || view.item?.validTo
												? `${view.item?.validFrom ? formatDate(view.item.validFrom) : "—"} – ${
														view.item?.validTo ? formatDate(view.item.validTo) : "—"
													}`
												: "—"}
										</Table.Cell>

										<Table.Cell className="table-cell-truncate">
											<span title={documentName(view.item?.documentId ?? null)}>
												{documentName(view.item?.documentId ?? null)}
											</span>
										</Table.Cell>

										<Table.Cell>
											<div className="flex justify-end">
												<Button variant="ghost" onPress={() => onRecord(view)}>
													{view.item ? "Update" : "Record"}
												</Button>
											</div>
										</Table.Cell>
									</Table.Row>
								))}
							</Table.Body>
						</Table.Content>
					</Table.ScrollContainer>
				</Table>
			) : null}
		</div>
	);
}
