import { createColumnHelper } from "@tanstack/react-table";
import type { CandidateProjection } from "@/api/models";
import type { appTableFeaturesType } from "@/components/table";
import { CandidateSourceBadge, ItemMark } from "@/components/ui";
import { formatDateTime } from "@/utlis/dateUtils";
import { CandidateActions } from "./CandidateActions";

const columnHelper = createColumnHelper<appTableFeaturesType, CandidateProjection>();

export type CanidateActions = {
	onEdit: (candidate: CandidateProjection) => void;
	onTag: (candidate: CandidateProjection) => void;
};

export function getColumns(actions: CanidateActions) {
	const columns = columnHelper.columns([
		columnHelper.display({
			id: "actions",
			header: () => null,
			meta: {
				width: "xxs",
				align: "right",
			},
			cell: ({ row }) => {
				const candidate = row.original;

				return (
					<div className="table-cell-content">
						<CandidateActions
							id={candidate.id}
							onEdit={() => {
								actions.onEdit(candidate);
							}}
							onTag={() => {
								actions.onTag(candidate);
							}}
						/>
					</div>
				);
			},
		}),
		columnHelper.accessor("fullName", {
			header: "Name",
			meta: {
				width: "xl",
			},
			cell: ({ row }) => (
				<div className="table-cell-content">
					<ItemMark name={row.original.fullName ?? row.original.email} />
					{row.original.fullName || "-"}
				</div>
			),
		}),
		columnHelper.accessor("email", {
			header: "Email",
			cell: ({ getValue }) => <span className="table-number truncate">{getValue()}</span>,
			meta: {
				width: "xl",
			},
		}),
		columnHelper.accessor("phoneNumber", {
			header: "Phone",
			meta: {
				width: "md",
			},
			cell: ({ getValue }) => <span className="table-number truncate">{getValue()}</span>,
		}),
		columnHelper.accessor("source", {
			header: "Source",
			meta: {
				width: "md",
			},
			cell: ({ getValue }) => <CandidateSourceBadge source={getValue()} />,
		}),

		columnHelper.accessor("createdAt", {
			header: "Created at",

			cell: ({ getValue }) => (
				<span className="table-number truncate table-header-xl">{formatDateTime(getValue())}</span>
			),
		}),
	]);
	return columns;
}
