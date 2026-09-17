import { createColumnHelper } from "@tanstack/react-table";

import type { JobDescriptionProjection } from "@/api/models";
import type { appTableFeaturesType } from "@/components/table";
import { JobDescriptionBadge } from "@/components/ui";
import { formatSalary } from "@/utlis";
import { formatDateTime } from "@/utlis/dateUtils";
import { JobsDescriptopnActions } from "./JobsDescriptopnActions";

const columnHelper = createColumnHelper<appTableFeaturesType, JobDescriptionProjection>();

export type Actions = {
	onChangeStatus: (item: JobDescriptionProjection) => void;
	onChangeRecruiter: (item: JobDescriptionProjection) => void;
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
						<JobsDescriptopnActions
							id={item.id}
							onChangeStatus={() => actions.onChangeStatus(item)}
							onChangeRecruiter={() => actions.onChangeRecruiter(item)}
						/>
					</div>
				);
			},
		}),

		columnHelper.accessor("title", {
			header: "Title",
			meta: {
				width: "xl",
			},
		}),

		columnHelper.accessor("status", {
			header: "Status",
			cell: ({ getValue }) => <JobDescriptionBadge status={getValue()} />,
			meta: {
				width: "sm",
			},
		}),

		columnHelper.accessor("recruiter", {
			header: "Responsible",
			meta: {
				width: "xl",
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
		columnHelper.accessor("employmentType", {
			header: "Type",
			meta: {
				width: "sm",
			},
		}),
		columnHelper.accessor("workMode", {
			header: "Mode",
			meta: {
				width: "sm",
			},
		}),

		columnHelper.accessor("salaryMin", {
			header: "Salary min",
			cell: ({ row, getValue }) => (
				<span className="table-number">
					{formatSalary(getValue() as number)} {row.original.currencyCode}
				</span>
			),
			meta: {
				width: "sm",
			},
		}),
		columnHelper.accessor("salaryMax", {
			header: "Salary max",
			cell: ({ row, getValue }) => (
				<span className="table-number">
					{formatSalary(getValue() as number)} {row.original.currencyCode}
				</span>
			),
			meta: {
				width: "sm",
			},
		}),
		columnHelper.accessor("company.name", {
			header: "Company",
			meta: {
				width: "xl",
			},
			cell: ({ getValue }) => <span className="max-w-50 truncate">{getValue()}</span>,
		}),

		columnHelper.accessor("createdAt", {
			header: "Created at",
			cell: ({ getValue }) => <span className="table-number">{formatDateTime(getValue())}</span>,
		}),
		columnHelper.accessor("modifiedAt", {
			header: "Updated at",
			cell: ({ getValue }) => <span className="table-number">{formatDateTime(getValue())}</span>,
		}),
	]);
	return columns;
}
