import { createColumnHelper } from "@tanstack/react-table";
import type { InterviewProjection } from "@/api/models";
import type { appTableFeaturesType } from "@/components/table";
import {
	InterviewFormatBadge,
	InterviewStatusBadge,
	InterviewTypeBadge,
	ItemMark,
} from "@/components/ui";
import { formatDateTime } from "@/utlis/dateUtils";
import type { InterviewActionsType } from "../forms";
import { InterviewActions } from "./InterviewActions";

const columnHelper = createColumnHelper<appTableFeaturesType, InterviewProjection>();

export type Actions = {
	onAction: (action: InterviewActionsType, item: InterviewProjection) => void;
};

export function getColumns(actions: Actions) {
	const columns = columnHelper.columns([
		columnHelper.display({
			id: "actions",
			header: () => null,
			meta: {
				width: "xxs",
			},
			cell: ({ row }) => {
				const item = row.original;

				return (
					<div className="table-cell-content">
						<InterviewActions onAction={(val) => actions.onAction(val, item)} />
					</div>
				);
			},
		}),

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
		columnHelper.accessor("scheduleAt", {
			header: "Schedule at",
			cell: ({ getValue }) => <span className="table-number">{formatDateTime(getValue())}</span>,
		}),
		columnHelper.accessor("applicantInfo.phoneNumber", {
			header: "Phone",
		}),
		columnHelper.accessor("status", {
			header: "Status",
			//InterviewStatusBadge
			cell: ({ getValue }) => <InterviewStatusBadge status={getValue()} />,
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

		columnHelper.accessor("interviewType", {
			header: "Type",
			cell: ({ getValue }) => <InterviewTypeBadge status={getValue()} />,
		}),

		columnHelper.accessor("format", {
			header: "Format",
			cell: ({ getValue }) => <InterviewFormatBadge status={getValue()} />,
		}),

		columnHelper.accessor("createdAt", {
			header: "Created at",
			cell: ({ getValue }) => <span className="table-number">{formatDateTime(getValue())}</span>,
		}),
	]);
	return columns;
}
