import { createColumnHelper } from "@tanstack/react-table";
import { MoreHorizontal } from "lucide-react";
import type { OpportunityProjection } from "@/api/models";
import type { appTableFeaturesType } from "@/components/table";
import { ItemMark } from "@/components/ui";

import { formatDateTime } from "@/utlis/dateUtils";

const columnHelper = createColumnHelper<appTableFeaturesType, OpportunityProjection>();

export function getColumns(onEdit?: (company: OpportunityProjection) => void) {
	const columns = columnHelper.columns([
		columnHelper.accessor("title", {
			header: "",
			meta: {
				width: "xl",
			},

			cell: ({ row }) => (
				<div className="table-cell-content">
					<ItemMark name={row.original.title} />
					<div className="data-meta truncate">{row.original.title}</div>
				</div>
			),
		}),
		columnHelper.accessor("description", {
			header: "Email",
			cell: ({ getValue }) => <div className="data-meta truncate">{getValue()}</div>,
			meta: {
				width: "2xl",
			},
		}),
		columnHelper.accessor("stage", {
			header: "Stage",
		}),
		columnHelper.accessor("expectedValue", {
			header: "Value",
			cell: ({ row, getValue }) => (
				<span className="table-number">
					{getValue()} {row.original.currencyCode}
				</span>
			),
		}),
		columnHelper.accessor("expectedCloseDate", {
			header: "Expected close date",
			cell: ({ getValue }) => <span className="table-number">{formatDateTime(getValue())}</span>,
		}),

		columnHelper.accessor("company.name", {
			header: "Company",
			cell: ({ getValue }) => <div className="data-meta truncate">{getValue()}</div>,
		}),

		columnHelper.accessor("responsible", {
			header: "Responsible",
			cell: ({ row }) => (
				<div className="table-cell-content">
					<div className="min-w-0">
						<div className="data-name">{row.original.responsible.fullname ?? ""}</div>

						<div className="data-meta">{row.original.responsible.email}</div>
					</div>
				</div>
			),
		}),
		columnHelper.accessor("createdAt", {
			header: "Created at",
			cell: ({ getValue }) => <span className="table-number">{formatDateTime(getValue())}</span>,
		}),

		columnHelper.display({
			id: "actions",
			header: () => null,
			cell: ({ row }) => {
				const user = row.original;

				if (!onEdit) {
					return null;
				}

				return (
					<div className="table-actions">
						<button
							type="button"
							className="table-action-button"
							aria-label={`Actions for ${user.id}`}
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
