import { ShieldCheck } from "lucide-react";
import type { ComplianceRequirementView, ProjectProjection } from "@/api/models";
import { Button, ComplianceStatusBadge, DetailOverviewHeader, EmptyState } from "@/components/ui";
import { formatDate } from "@/utlis/dateUtils";
import { complianceRequirements } from "../../../types";
import { useGetComplianceCatalogue } from "../../hooks";

interface ProjectComplianceSectionProps {
	project: ProjectProjection;
	onRecord: (view: ComplianceRequirementView) => void;
}

/**
 * The catalogue, not the recorded items: a requirement nobody has touched yet exists only in the
 * catalogue, and those are exactly the ones worth seeing. The API already merges the two, so a
 * row without an item is simply "not started".
 */
export function ProjectComplianceSection({ project, onRecord }: ProjectComplianceSectionProps) {
	const query = useGetComplianceCatalogue(project.id);

	const documentName = (documentId: string | null) =>
		project.documents.find((document) => document.documentId === documentId)?.fileName ?? "—";

	return (
		<div className="data-overview">
			<DetailOverviewHeader
				title="Compliance"
				description="What this country and this engagement type require, and where each of them stands."
			/>

			{query.isLoading ? <p className="project-role-empty">Loading…</p> : null}

			{query.isError ? (
				<p className="form-error">The compliance catalogue could not be loaded.</p>
			) : null}

			{query.data && query.data.length === 0 ? (
				<EmptyState
					title="No country-specific requirements for this engagement"
					description="Work in the country where the agency is established does not trigger a host state's duties. That is an answer, not missing data."
				>
					<ShieldCheck size={24} />
				</EmptyState>
			) : null}

			{query.data && query.data.length > 0 ? (
				<table className="table">
					<thead>
						<tr>
							<th>Requirement</th>
							<th>Status</th>
							<th>Reference</th>
							<th>Valid</th>
							<th>Document</th>
							<th />
						</tr>
					</thead>

					<tbody>
						{query.data.map((view) => (
							<tr key={view.requirement}>
								<td>{complianceRequirements[view.requirement]}</td>

								<td>
									<ComplianceStatusBadge status={view.item?.status ?? "NotStarted"} />
								</td>

								<td>{view.item?.referenceNumber ?? "—"}</td>

								<td>
									{view.item?.validFrom || view.item?.validTo
										? `${view.item?.validFrom ? formatDate(view.item.validFrom) : "—"} – ${
												view.item?.validTo ? formatDate(view.item.validTo) : "—"
											}`
										: "—"}
								</td>

								<td>{documentName(view.item?.documentId ?? null)}</td>

								<td>
									<div className="flex justify-end">
										<Button variant="ghost" onClick={() => onRecord(view)}>
											{view.item ? "Update" : "Record"}
										</Button>
									</div>
								</td>
							</tr>
						))}
					</tbody>
				</table>
			) : null}
		</div>
	);
}
