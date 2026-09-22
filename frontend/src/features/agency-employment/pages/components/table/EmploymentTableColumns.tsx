import { createColumnHelper } from "@tanstack/react-table";
import type { AgencyEmploymentProjection } from "@/api/models";
import type { appTableFeaturesType } from "@/components/table";
import { ContractTypeBadge, ItemMark } from "@/components/ui";
import { formatDate } from "@/utlis/dateUtils";
import { isEnded } from "../../../types";
import { EmploymentActions } from "./EmploymentActions";

const columnHelper = createColumnHelper<appTableFeaturesType, AgencyEmploymentProjection>();

type ColumnHandlers = {
	onChangeTerms: (employment: AgencyEmploymentProjection) => void;
	onEnd: (employment: AgencyEmploymentProjection) => void;
};

export function getColumns({ onChangeTerms, onEnd }: ColumnHandlers) {
	return columnHelper.columns([
		columnHelper.display({
			id: "actions",
			header: () => null,
			meta: { width: "xxs" },
			cell: ({ row }) => (
				<EmploymentActions
					userId={row.original.userId}
					name={`${row.original.user.firstName} ${row.original.user.lastName}`}
					isEnded={isEnded(row.original)}
					onChangeTerms={() => onChangeTerms(row.original)}
					onEnd={() => onEnd(row.original)}
				/>
			),
		}),

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

		/* The badge carries both facts: what they are on, and whether it means hours are owed. */
		columnHelper.accessor("contractType", {
			header: "Contract",
			meta: { width: "xl" },
			cell: ({ getValue }) => <ContractTypeBadge contractType={getValue()} />,
		}),

		columnHelper.accessor("weeklyHours", {
			header: "Weekly hours",
			meta: { width: "sm", align: "right" },
			cell: ({ getValue }) => {
				const hours = getValue();

				return <span className="table-figure">{hours === null ? "—" : `${hours} h`}</span>;
			},
		}),

		columnHelper.accessor("startsOn", {
			header: "Engaged",
			meta: { width: "md" },
			cell: ({ row, getValue }) => (
				<span>
					{formatDate(getValue())}
					{row.original.endsOn ? ` – ${formatDate(row.original.endsOn)}` : ""}
				</span>
			),
		}),

		columnHelper.display({
			id: "state",
			header: "State",
			meta: { width: "sm" },
			cell: ({ row }) => (
				<span className={`badge ${isEnded(row.original) ? "badge-closed" : "badge-active"}`}>
					{isEnded(row.original) ? "Ended" : "Running"}
				</span>
			),
		}),
	]);
}
