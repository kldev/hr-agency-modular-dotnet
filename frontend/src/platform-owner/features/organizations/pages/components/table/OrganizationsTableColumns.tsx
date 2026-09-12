import { createColumnHelper } from "@tanstack/react-table";

import type { OrganizationProjection } from "@/api/models";
import type { appTableFeaturesType } from "@/components/table";
import { ItemMark } from "@/components/ui";
import { formatDateTime } from "@/utlis/dateUtils";
import { OrganizationsActions } from "./OrganizationsActions";

const columnHelper = createColumnHelper<appTableFeaturesType, OrganizationProjection>();

export type Actions = {
	onAddUser: (item: OrganizationProjection) => void;
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
					<div className="table-cell-content w-87.5">
						<OrganizationsActions onAddUser={() => actions.onAddUser(item)} />
					</div>
				);
			},
		}),
		columnHelper.accessor("name", {
			header: "Name",
			meta: {
				width: "2xl",
			},

			cell: ({ row, getValue }) => (
				<div className="table-cell-content w-87.5">
					<ItemMark name={row.original.name} />

					<div>
						<div className="data-name">{getValue()}</div>
					</div>
				</div>
			),
		}),
		columnHelper.accessor("slug", {
			header: "Slug",
			cell: ({ getValue }) => <span className="table-number truncate">{getValue()}</span>,
			meta: {
				width: "xl",
			},
		}),

		columnHelper.accessor("createdAt", {
			header: "Created at",
			cell: ({ getValue }) => <span className="table-number">{formatDateTime(getValue())}</span>,
		}),
	]);
	return columns;
}
