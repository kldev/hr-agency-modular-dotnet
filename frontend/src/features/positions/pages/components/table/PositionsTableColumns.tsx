import { Link } from "@tanstack/react-router";
import { createColumnHelper } from "@tanstack/react-table";
import type { PositionListItem } from "@/api/models";
import type { appTableFeaturesType } from "@/components/table";
import { ItemMark } from "@/components/ui";
import { PositionStaffing } from "../../../components/PositionStaffing";
import { rateUnitShort, workerContractTypes } from "../../../types";
import { PositionActions } from "./PositionActions";

const columnHelper = createColumnHelper<appTableFeaturesType, PositionListItem>();

type positionsActions = {
	onEdit: (position: PositionListItem) => void;
	onArchive: (position: PositionListItem) => void;
	onRestore: (position: PositionListItem) => void;
};

export function getColumns(actions: positionsActions) {
	return columnHelper.columns([
		columnHelper.accessor("name", {
			header: "Position",
			meta: { width: "2xl" },

			cell: ({ row, getValue }) => {
				const position = row.original;

				return (
					<div className="table-cell-content">
						<PositionActions
							isArchived={position.isArchived}
							onEdit={() => actions.onEdit(position)}
							onArchive={() => actions.onArchive(position)}
							onRestore={() => actions.onRestore(position)}
						/>

						<ItemMark name={position.name} />

						<div>
							<div className="data-name">{getValue()}</div>

							{/* Both names are shown when they differ: that difference is the reason two roles
							    with the same contract wording are two roles. */}
							<div className="data-meta max-w-67.5 truncate">
								{position.contractName && position.contractName !== position.name
									? `on the contract: ${position.contractName}`
									: workerContractTypes[position.contractType]}
							</div>
						</div>
					</div>
				);
			},
		}),

		columnHelper.accessor("projectName", {
			header: "Project",
			cell: ({ row, getValue }) => (
				<Link
					to="/app/projects/$id"
					params={{ id: row.original.projectId }}
					search={{ search: undefined, tab: "positions" as const }}
				>
					{getValue()}
				</Link>
			),
		}),

		columnHelper.accessor("clientCompanyName", {
			header: "Client",
		}),

		columnHelper.accessor("workCountry", {
			header: "Country",
			meta: { align: "center", width: "xxs" },
		}),

		columnHelper.accessor("contractType", {
			header: "We sign",
			meta: { width: "sm" },
			cell: ({ getValue }) => workerContractTypes[getValue()],
		}),

		columnHelper.accessor("proposedRate", {
			header: "Rate",
			meta: { width: "sm" },
			cell: ({ getValue }) => {
				const rate = getValue();

				return rate ? `${rate.amount} ${rate.currency}/${rateUnitShort[rate.unit]}` : "—";
			},
		}),

		columnHelper.accessor("assignedCount", {
			header: "Staffed",
			meta: { align: "center", width: "sm" },
			cell: ({ row, getValue }) => (
				<PositionStaffing assigned={getValue()} planned={row.original.plannedHeadcount} />
			),
		}),

		columnHelper.accessor("isArchived", {
			header: "",
			meta: { align: "center", width: "xs" },
			cell: ({ getValue }) =>
				getValue() ? <span className="badge badge-inactive">Archived</span> : null,
		}),
	]);
}
