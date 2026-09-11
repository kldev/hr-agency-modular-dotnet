import { useTable } from "@tanstack/react-table";
import type { UserProjection } from "@/api/models";
import { TableHeaderGroup } from "@/components/table/TableHeaderGroup";
import { appTableFeatures } from "@/components/table/tableFeatures";
import { getColumns } from "./UsersTableColumns";

interface UseresTableProps {
	users: UserProjection[];
	onEdit?: (user: UserProjection) => void;
	onDelete?: (user: UserProjection) => void;
}

export function UseresTable({ users, onEdit }: UseresTableProps) {
	const table = useTable(
		{
			features: appTableFeatures,
			columns: getColumns(onEdit),
			data: users,
			getRowId: (user) => user.id,
			enableSorting: false,
		},
		(state) => ({
			sorting: state.sorting,
		}),
	);

	return (
		<div className="table-container">
			<table className="table">
				<thead>
					{table.getHeaderGroups().map((headerGroup) => (
						<TableHeaderGroup key={headerGroup.id} table={table} group={headerGroup} />
					))}
				</thead>

				<tbody>
					{table.getRowModel().rows.map((row) => (
						<tr key={row.id}>
							{row.getAllCells().map((cell) => {
								const meta = cell.column.columnDef.meta;
								const className =
									meta?.align === "right"
										? `table-number ${meta?.className}`
										: `${meta?.className}`;
								return (
									<td key={cell.id} className={className}>
										<table.FlexRender cell={cell} />
									</td>
								);
							})}
						</tr>
					))}
				</tbody>
			</table>
		</div>
	);
}
