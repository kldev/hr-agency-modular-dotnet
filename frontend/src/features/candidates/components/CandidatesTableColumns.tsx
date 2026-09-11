import { createColumnHelper } from "@tanstack/react-table";
import { MoreHorizontal } from "lucide-react";
import type { CandidateProjection } from "@/api/models";
import type { appTableFeaturesType } from "@/components/table";
import { ItemMark } from "@/components/ui";
import { formatDateTime } from "@/utlis/dateUtils";

const columnHelper = createColumnHelper<appTableFeaturesType, CandidateProjection>();

export function getColumns(onEdit?: (company: CandidateProjection) => void) {
	const columns = columnHelper.columns([
		columnHelper.accessor("fullName", {
			header: "",
			meta: {
				width: "xs",
			},

			cell: ({ row }) => (
				<div className="table-cell-content w-87.5">
					<ItemMark name={row.original.fullName ?? row.original.email} />
				</div>
			),
		}),
		columnHelper.accessor("email", {
			header: "Email",
			cell: ({ getValue }) => <a href={`email:${getValue()}`} className="table-number truncate">{getValue()}</a>,
			meta: {
				width: "xl",
			},
		}),
		columnHelper.accessor("phoneNumber", {
			header: "Phone",
			cell: ({ getValue }) => <a href={`tel:${getValue()}`} className="table-number truncate">{getValue()}</a>,
		}),
		columnHelper.accessor("source", {
			header: "Source",
		}),
		columnHelper.accessor("fullName", {
			header: "Name",
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
				const user = row.original;

				if (!onEdit) {
					return null;
				}

				return (
					<div className="table-actions">
						<button
							type="button"
							className="table-action-button"
							aria-label={`Actions for ${user.email}`}
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
