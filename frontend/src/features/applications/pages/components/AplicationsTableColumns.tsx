import { createColumnHelper } from "@tanstack/react-table";
import { MoreHorizontal } from "lucide-react";
import type { JobApplicationProjection } from "@/api/models";
import type { appTableFeaturesType } from "@/components/table";
import { ItemMark } from "@/components/ui";
import { ApplicationBadge } from "@/components/ui/Badge";
import { formatDateTime } from "@/utlis/dateUtils";

const columnHelper = createColumnHelper<appTableFeaturesType, JobApplicationProjection>();

export function getColumns(onEdit?: (company: JobApplicationProjection) => void) {
	const columns = columnHelper.columns([
		columnHelper.accessor("applicantFullName", {
			header: "",
			meta: {
				width: "xs",
			},

			cell: ({ row }) => (
				<div className="table-cell-content w-87.5">
					<ItemMark name={row.original.applicantFullName ?? row.original.applicantEmail} />
				</div>
			),
		}),
		columnHelper.accessor("applicantEmail", {
			header: "Email",
			cell: ({ getValue }) => (
				<a href={`email:${getValue()}`} className="table-number truncate">
					{getValue()}
				</a>
			),
			meta: {
				width: "xl",
			},
		}),
		columnHelper.accessor("applicantPhone", {
			header: "Phone",
			cell: ({ getValue }) => (
				<a href={`tel:${getValue()}`} className="table-number truncate">
					{getValue()}
				</a>
			),
		}),
		columnHelper.accessor("status", {
			header: "Status",
			cell: ({ getValue }) => <ApplicationBadge status={getValue()} />,
		}),
		columnHelper.accessor("source", {
			header: "Source",
		}),
		columnHelper.accessor("applicantFullName", {
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
		columnHelper.accessor("jobPostTitle", {
			header: "Job post",
			cell: ({ getValue }) => (
				<a href={`tel:${getValue()}`} className="table-number truncate">
					{getValue()}
				</a>
			),
		}),
		columnHelper.accessor("company", {
			header: "Company",
			cell: ({ getValue }) => (
				<a href={`tel:${getValue()}`} className="table-number truncate">
					{getValue().name}
				</a>
			),
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
							aria-label={`Actions for ${user.applicantEmail}`}
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
