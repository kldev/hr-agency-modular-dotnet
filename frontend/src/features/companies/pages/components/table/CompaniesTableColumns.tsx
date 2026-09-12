import { createColumnHelper } from "@tanstack/react-table";

import type { CompanyProjection } from "@/api/models";
import type { appTableFeaturesType } from "@/components/table";
import { ItemMark } from "@/components/ui";
import { formatDateTime } from "@/utlis/dateUtils";
import { CompanyActions } from "./CompanyActions";

const columnHelper = createColumnHelper<appTableFeaturesType, CompanyProjection>();

type companiesActions = {
	onEdit?: (company: CompanyProjection) => void;
	onAddContact?: (company: CompanyProjection) => void;
};

export function getColumns(actions: companiesActions = {}) {
	const columns = columnHelper.columns([
		columnHelper.accessor("name", {
			header: "Company",
			meta: {
				width: "2xl",
			},

			cell: ({ row, getValue }) => {
				const company = row.original;
				return (
					<div className="table-cell-content">
						<CompanyActions
							id={company.id}
							onEdit={() => actions.onEdit?.(company)}
							onAddContact={() => actions.onAddContact?.(company)}
						/>
						<ItemMark name={row.original.name} />

						<div>
							<div className="data-name">{getValue()}</div>

							<div className="data-meta max-w-67.5 truncate">
								{row.original.countryCode} · {row.original.website}
							</div>
						</div>
					</div>
				);
			},
		}),

		columnHelper.accessor("industry", {
			header: "Industry",
		}),

		columnHelper.accessor("taxId", {
			header: "Tax",
		}),
		columnHelper.accessor("contact", {
			header: "Contact",
			meta: {
				width: "xl",
			},

			cell: ({ getValue }) => (
				<div className="table-cell-content">
					<div>
						<div className="data-name">{getValue()?.fullname}</div>

						<div className="data-meta truncate">{getValue()?.email}</div>
						<div className="data-meta truncate">{getValue()?.jobTitle}</div>
					</div>
				</div>
			),
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
			cell: ({ getValue }) => <span className="table-number">{formatDateTime(getValue())}</span>,
		}),
	]);
	return columns;
}
