import { createColumnHelper } from "@tanstack/react-table";
import { MoreHorizontal } from "lucide-react";
import type { InterviewProjection } from "@/api/models";
import type { appTableFeaturesType } from "@/components/table";
import { ItemMark } from "@/components/ui";
import { formatDateTime } from "@/utlis/dateUtils";

const columnHelper = createColumnHelper<appTableFeaturesType, InterviewProjection>();

export function getColumns(onEdit?: (company: InterviewProjection) => void) {
	const columns = columnHelper.columns([
		columnHelper.accessor("applicantInfo", {
			header: "Applicant",
			meta: {
				width: "2xl",
			},

			cell: ({ row, getValue }) => (
				<div className="table-cell-content w-87.5">
					<ItemMark name={row.original.applicantInfo.fullName} />

					<div>
						<div className="data-name">{getValue().fullName}</div>
						<a href={`email:${getValue().email}`} className="data-meta">
							{getValue().email}
						</a>
					</div>
				</div>
			),
		}),
		columnHelper.accessor("applicantInfo.phoneNumber", {
			header: "Phone",
		}),
		columnHelper.accessor("status", {
			header: "Status",
		}),

		columnHelper.accessor("scheduleAt", {
			header: "Schedule at",
			cell: ({ getValue }) => <span className="table-number">{formatDateTime(getValue())}</span>,
		}),

		columnHelper.accessor("interviewer", {
			header: "Interviewer",
			meta: {
				width: "2xl",
			},

			cell: ({ getValue }) => (
				<div className="table-cell-content w-87.5">
					<div>
						<div className="data-name">{getValue().fullname}</div>
						<a href={`email:${getValue().email}`} className="data-meta">
							{getValue().email}
						</a>
					</div>
				</div>
			),
		}),

		columnHelper.accessor("format", {
			header: "Format",
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
							aria-label={`Actions for ${item.applicantInfo.email}`}
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
