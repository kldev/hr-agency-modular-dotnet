import { createColumnHelper } from "@tanstack/react-table";
import { MoreHorizontal } from "lucide-react";
import type { CompanyProjection } from "@/api/models";
import type { appTableFeaturesType } from "@/components/table";

const columnHelper = createColumnHelper<appTableFeaturesType, CompanyProjection>();

function CompanyMark({ company }: { company: CompanyProjection }) {
	const initials = company.name
		.split(" ")
		.slice(0, 2)
		.map((part) => part[0])
		.join("")
		.toUpperCase();

	return <span className="data-avatar">{initials}</span>;
}

export function getColumns(onEdit?: (company: CompanyProjection) => void) {
	const columns = columnHelper.columns([
		columnHelper.accessor("name", {
			header: "Company",
			meta: {
				width: "2xl",
			},

			cell: ({ row, getValue }) => (
				<div className="table-cell-content w-87.5">
					<CompanyMark company={row.original} />

					<div>
						<div className="data-name">{getValue()}</div>

						<div className="data-meta max-w-87.5 truncate">
							{row.original.countryCode} · {row.original.website}
						</div>
					</div>
				</div>
			),
		}),

		columnHelper.accessor("industry", {
			header: "Industry",
		}),

		columnHelper.accessor("taxId", {
			header: "Tax",
		}),

		columnHelper.accessor("activeJobsPostCount", {
			header: "Active job posts",
			cell: ({ getValue }) => <span className="table-number">{getValue()}</span>,
			meta: { align: "right" },
		}),

		columnHelper.accessor("applicantsCount", {
			header: "Applicants count",
			cell: ({ getValue }) => <span className="table-number">{getValue()}</span>,
			meta: { align: "right" },
		}),

		columnHelper.accessor("modifiedAt", {
			header: "Updated",
		}),

		columnHelper.display({
			id: "actions",
			header: () => null,
			cell: ({ row }) => {
				const company = row.original;

				if (!onEdit) {
					return null;
				}

				return (
					<div className="table-actions">
						<button
							type="button"
							className="table-action-button"
							aria-label={`Actions for ${company.name}`}
						>
							<MoreHorizontal className="size-4" />
						</button>
					</div>
				);
			},
		}),
	]);
	return columns;
}
