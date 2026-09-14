import { createColumnHelper } from "@tanstack/react-table";

import type { JobApplicationProjection } from "@/api/models";
import type { appTableFeaturesType } from "@/components/table";
import { ApplicationBadge, CandidateSourceBadge } from "@/components/ui/Badge";
import { formatDateTime } from "@/utlis/dateUtils";
import type { JobApplicationsActionsType } from "../forms";
import { ApplicationsActions } from "./ApplicationsActions";

const columnHelper = createColumnHelper<appTableFeaturesType, JobApplicationProjection>();

export type Actions = {
	onAction: (action: JobApplicationsActionsType, item: JobApplicationProjection) => void;
};

export function getColumns(actions: Actions) {
	const columns = columnHelper.columns([
		columnHelper.display({
			id: "actions",
			meta: {
				width: "xxs",
			},
			header: () => null,
			cell: ({ row }) => {
				const item = row.original;

				return (
					<div className="table-cell-content w-87.5">
						<ApplicationsActions id={item.id} onAction={(a) => actions.onAction(a, item)} />
					</div>
				);
			},
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
			meta: {
				width: "md",
			},
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
			cell: ({ getValue }) => <CandidateSourceBadge source={getValue()} />,
		}),
		columnHelper.accessor("applicantFullName", {
			header: "Name",
			cell: ({ getValue }) => <span className="table-number truncate">{getValue()}</span>,
			meta: {
				width: "md",
			},
		}),

		columnHelper.accessor("jobPostTitle", {
			header: "Job post",
			meta: {
				width: "xl",
			},
			cell: ({ getValue }) => (
				<a href={`tel:${getValue()}`} className="table-number truncate max-w-">
					{getValue()}
				</a>
			),
		}),
		columnHelper.accessor("company", {
			header: "Company",
			meta: {
				width: "xl",
			},
			cell: ({ getValue }) => (
				<a href={`tel:${getValue()}`} className="table-number truncate">
					{getValue().name}
				</a>
			),
		}),
		columnHelper.accessor("createdAt", {
			header: "Created at",
			cell: ({ getValue }) => <span className="table-number">{formatDateTime(getValue())}</span>,
		}),
	]);
	return columns;
}
