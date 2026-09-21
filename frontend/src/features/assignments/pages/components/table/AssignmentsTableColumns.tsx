import { Link } from "@tanstack/react-router";
import { createColumnHelper } from "@tanstack/react-table";
import type { AssignmentProjection } from "@/api/models";
import type { appTableFeaturesType } from "@/components/table";
import { AssignmentStatusBadge, ItemMark } from "@/components/ui";
import { ComplianceChip, engagementTypes } from "@/features/compliance";
import { formatPeriod } from "@/utlis/formatRecord";

const columnHelper = createColumnHelper<appTableFeaturesType, AssignmentProjection>();

export function getColumns() {
	return columnHelper.columns([
		columnHelper.accessor("workerFullName", {
			header: "Worker",
			meta: { width: "xl" },

			cell: ({ row, getValue }) => {
				const assignment = row.original;

				return (
					<div className="table-cell-content">
						<ItemMark name={assignment.workerFullName} />

						<div>
							<div className="data-name">
								<Link
									to="/app/workers/$id"
									params={{ id: assignment.workerId }}
									search={{ search: undefined }}
								>
									{getValue()}
								</Link>
							</div>

							<div className="data-meta max-w-67.5 truncate">{assignment.position}</div>
						</div>
					</div>
				);
			},
		}),

		columnHelper.accessor("projectName", {
			header: "Project",
			meta: { width: "xl" },

			cell: ({ row, getValue }) => (
				<div>
					<div className="flex flex-col">
						<Link
							to="/app/assignments/$id"
							params={{ id: row.original.id }}
							search={{ search: undefined }}
						>
							{getValue()}
						</Link>

						<div className="data-meta max-w-67.5 truncate">{row.original.clientCompanyName}</div>
					</div>
				</div>
			),
		}),

		/* Which of our companies posted them: the one that has to issue the A1. */
		columnHelper.accessor("deliveringEntityName", {
			header: "Posted by",
			meta: { width: "sm" },
		}),

		columnHelper.accessor("workCountry", {
			header: "Country",
			meta: { align: "center", width: "xxs" },
		}),

		columnHelper.accessor("engagementType", {
			header: "Engagement",
			meta: { width: "sm" },
			cell: ({ getValue }) => engagementTypes[getValue()],
		}),

		columnHelper.accessor("startsOn", {
			header: "Period",
			meta: { width: "sm" },
			cell: ({ row, getValue }) => (
				<span className="table-number">{formatPeriod(getValue(), row.original.endsOn)}</span>
			),
		}),

		columnHelper.accessor("status", {
			header: "Status",
			meta: { width: "sm" },
			cell: ({ getValue }) => <AssignmentStatusBadge status={getValue()} />,
		}),

		columnHelper.accessor("complianceOutstandingCount", {
			header: "Compliance",
			meta: { align: "center", width: "sm" },
			cell: ({ row, getValue }) => (
				<ComplianceChip
					outstanding={getValue()}
					nextExpiryOn={row.original.nextComplianceExpiryOn}
				/>
			),
		}),
	]);
}
