import { Link } from "@tanstack/react-router";
import { createColumnHelper } from "@tanstack/react-table";
import { Lock } from "lucide-react";
import type { WorkerProjection } from "@/api/models";
import { getCountryLabel } from "@/components/labels";
import type { appTableFeaturesType } from "@/components/table";
import { ItemMark, WorkerStatusBadge } from "@/components/ui";
import { WorkerExpiryChip } from "../WorkerExpiryChip";
import { WorkerActions } from "./WorkerActions";

const columnHelper = createColumnHelper<appTableFeaturesType, WorkerProjection>();

type workersActions = {
	onEdit?: (worker: WorkerProjection) => void;
	onChangeStatus?: (worker: WorkerProjection) => void;
	onPlanAssignment?: (worker: WorkerProjection) => void;
};

export function getColumns(actions: workersActions) {
	return columnHelper.columns([
		columnHelper.accessor("fullName", {
			header: "Worker",
			meta: { width: "2xl" },

			cell: ({ row, getValue }) => {
				const worker = row.original;

				return (
					<div className="table-cell-content">
						<WorkerActions
							id={worker.id ?? ""}
							onEdit={actions.onEdit && (() => actions.onEdit?.(worker))}
							onChangeStatus={actions.onChangeStatus && (() => actions.onChangeStatus?.(worker))}
							onPlanAssignment={
								actions.onPlanAssignment && (() => actions.onPlanAssignment?.(worker))
							}
						/>

						<ItemMark name={worker.fullName ?? ""} />

						<div>
							<div className="data-name">
								<Link
									to="/app/workers/$id"
									params={{ id: worker.id ?? "" }}
									search={{ search: undefined, tab: undefined }}
								>
									{getValue()}
								</Link>
							</div>

							<div className="data-meta max-w-67.5 truncate">
								{worker.currentProjectName ?? "Not on a project"}
							</div>
						</div>
					</div>
				);
			},
		}),

		columnHelper.accessor("citizenship", {
			header: "Citizenship",
			meta: { width: "sm" },

			cell: ({ row, getValue }) => (
				<span className="table-cell-content">
					{getCountryLabel(getValue() ?? "")}

					{/* Legalisation is a fact about the passport, not about the file, and it decides
					    whether this person has a permit section at all. */}
					{row.original.requiresLegalisation ? (
						<Lock size={13} className="ml-1" aria-label="Needs legalisation" />
					) : null}
				</span>
			),
		}),

		columnHelper.accessor("status", {
			header: "Status",
			meta: { width: "sm" },
			cell: ({ getValue }) => <WorkerStatusBadge status={getValue() ?? "Recruitment"} />,
		}),

		columnHelper.accessor("currentWorkCountry", {
			header: "Works in",
			meta: { align: "center", width: "xxs" },
			cell: ({ getValue }) => getValue() ?? "—",
		}),

		columnHelper.accessor("openAssignmentCount", {
			header: "Postings",
			meta: { align: "center", width: "xxs" },
			cell: ({ row, getValue }) => (
				<span className="table-number" title="Open of total">
					{Number(getValue() ?? 0)} / {Number(row.original.assignmentCount ?? 0)}
				</span>
			),
		}),

		columnHelper.accessor("identityDocumentValidUntil", {
			header: "Expiry",
			meta: { align: "center", width: "sm" },
			cell: ({ row, getValue }) => (
				<WorkerExpiryChip
					identityDocumentValidUntil={getValue()}
					nextAuthorisationExpiryOn={row.original.nextAuthorisationExpiryOn}
				/>
			),
		}),
	]);
}
