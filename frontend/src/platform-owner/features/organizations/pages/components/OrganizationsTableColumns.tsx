import { createColumnHelper } from "@tanstack/react-table";
import { MoreHorizontal } from "lucide-react";
import type { OrganizationProjection } from "@/api/models";
import type { appTableFeaturesType } from "@/components/table";
import { ItemMark } from "@/components/ui";
import { formatDateTime } from "@/utlis/dateUtils";

const columnHelper = createColumnHelper<appTableFeaturesType, OrganizationProjection>();

export function getColumns(onEdit?: (company: OrganizationProjection) => void) {
	const columns = columnHelper.columns([
		columnHelper.accessor("name", {
			header: "Name",
			meta: {
				width: "2xl",
			},

			cell: ({ row, getValue }) => (
				<div className="table-cell-content w-87.5">
					<ItemMark name={row.original.name} />

					<div>
						<div className="data-name">{getValue()}</div>
					</div>
				</div>
			),
		}),
		columnHelper.accessor("slug", {
			header: "Slug",
			cell: ({ getValue }) => <span className="table-number truncate">{getValue()}</span>,
			meta: {
				width: "xl",
			},
		}),

		columnHelper.accessor("createdAt", {
			header: "Created at",
			cell: ({ getValue }) => <span className="table-number">{formatDateTime(getValue())}</span>,
		}),

		columnHelper.display({
			id: "actions",
			header: () => null,
			cell: ({ row }) => {
				const item = row.original;

				if (!onEdit) {
					return null;
				}

				return (
					<div className="table-actions">
						<button
							type="button"
							className="table-action-button"
							aria-label={`Actions for ${item.name}`}
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
