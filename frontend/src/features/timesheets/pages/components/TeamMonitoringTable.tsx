import { createColumnHelper, useTable } from "@tanstack/react-table";
import type { TeamTimeSheetRow } from "@/api/models";
import type { appTableFeaturesType } from "@/components/table";
import MainTable from "@/components/table/MainTable";
import { appTableFeatures } from "@/components/table/tableFeatures";
import { ItemMark, TimeSheetStatusOrNotStartedBadge } from "@/components/ui";
import { formatDate } from "@/utlis/dateUtils";
import { formatMinutes, type MonthInView, workingDaysInMonth } from "../../types";

const columnHelper = createColumnHelper<appTableFeaturesType, TeamTimeSheetRow>();

interface Props {
	rows: TeamTimeSheetRow[];
	month: MonthInView;
}

/**
 * The screen somebody opens daily, as opposed to the approval queue they open once a month. Its
 * whole point is the row nobody has written on yet, so those rows are not filtered out anywhere -
 * the backend puts them in deliberately and they stay in the same order as everybody else.
 */
export function TeamMonitoringTable({ rows, month }: Props) {
	const workingDays = workingDaysInMonth(month);

	const table = useTable(
		{
			features: appTableFeatures,
			data: rows,
			getRowId: (row) => row.userId,
			enableSorting: false,
			columns: columnHelper.columns([
				columnHelper.display({
					id: "person",
					header: "Person",
					meta: { width: "xl" },
					cell: ({ row }) => {
						const name = `${row.original.user.firstName} ${row.original.user.lastName}`;

						return (
							<div className="table-cell-content">
								<ItemMark name={name} />

								<span className="truncate" title={row.original.user.email}>
									{name}
								</span>
							</div>
						);
					},
				}),

				columnHelper.accessor("status", {
					header: "Status",
					meta: { width: "sm" },
					cell: ({ getValue }) => <TimeSheetStatusOrNotStartedBadge status={getValue()} />,
				}),

				columnHelper.accessor("totalMinutes", {
					header: "Hours",
					meta: { width: "ssm", align: "right" },
					cell: ({ getValue }) => (
						<span className="table-figure">{formatMinutes(Number(getValue()))}</span>
					),
				}),

				/* Filled against what could have been filled - a bare count says nothing in February. */
				columnHelper.accessor("filledDays", {
					header: "Days",
					meta: { width: "ssm", align: "right" },
					cell: ({ getValue }) => (
						<span className="table-figure">
							{Number(getValue())} / {workingDays}
						</span>
					),
				}),

				columnHelper.accessor("lastEntryOn", {
					header: "Last entry",
					meta: { width: "md" },
					cell: ({ getValue }) => formatDate(getValue()),
				}),

				columnHelper.accessor("submittedAt", {
					header: "Sent",
					meta: { width: "md" },
					cell: ({ getValue }) => formatDate(getValue()),
				}),
			]),
		},
		(state) => ({ sorting: state.sorting }),
	);

	return <MainTable table={table} className="table-wide" />;
}
