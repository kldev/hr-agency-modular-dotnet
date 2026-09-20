import { createColumnHelper } from "@tanstack/react-table";
import type { LegalEntityProjection } from "@/api/models";
import type { appTableFeaturesType } from "@/components/table";
import { ItemMark, LegalEntityStatusBadge } from "@/components/ui";
import { formatDate } from "@/utlis/dateUtils";
import { isTrading } from "../../../types";
import { LegalEntityActions } from "./LegalEntityActions";

const columnHelper = createColumnHelper<appTableFeaturesType, LegalEntityProjection>();

type ColumnHandlers = {
	onEdit: (entity: LegalEntityProjection) => void;
	onClose: (entity: LegalEntityProjection) => void;
};

export function getColumns({ onEdit, onClose }: ColumnHandlers) {
	return columnHelper.columns([
		columnHelper.display({
			id: "actions",
			header: () => null,
			meta: { width: "xxs" },
			cell: ({ row }) => (
				<LegalEntityActions
					id={row.original.id}
					name={row.original.name}
					isClosed={Boolean(row.original.activeTo)}
					onEdit={() => onEdit(row.original)}
					onClose={() => onClose(row.original)}
				/>
			),
		}),

		columnHelper.accessor("name", {
			header: "Entity",
			meta: { width: "xl" },
			cell: ({ row, getValue }) => (
				<div className="table-cell-content">
					<ItemMark name={getValue()} />

					<span className="truncate" title={row.original.legalName}>
						{getValue()}
					</span>
				</div>
			),
		}),

		columnHelper.accessor("taxId", {
			header: "Tax ID",
			meta: { width: "md" },
		}),

		columnHelper.accessor("registeredAddress", {
			header: "Registered in",
			meta: { width: "sm" },
			cell: ({ getValue }) => getValue().countryCode.toUpperCase(),
		}),

		columnHelper.display({
			id: "president",
			header: "President",
			meta: { width: "md" },
			cell: ({ row }) => (
				<span className="truncate">
					{row.original.president.firstName} {row.original.president.lastName}
				</span>
			),
		}),

		columnHelper.display({
			id: "accounts",
			header: "Accounts",
			meta: { width: "ssm", align: "right" },
			cell: ({ row }) => row.original.bankAccounts.length,
		}),

		columnHelper.accessor("activeFrom", {
			header: "Trading",
			meta: { width: "md" },
			cell: ({ row, getValue }) => (
				<span>
					{formatDate(getValue())}
					{row.original.activeTo ? ` – ${formatDate(row.original.activeTo)}` : ""}
				</span>
			),
		}),

		columnHelper.display({
			id: "status",
			header: "Status",
			meta: { width: "sm" },
			cell: ({ row }) => (
				<LegalEntityStatusBadge
					isTrading={isTrading(row.original.activeFrom, row.original.activeTo)}
				/>
			),
		}),
	]);
}
