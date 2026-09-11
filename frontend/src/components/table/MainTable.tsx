import type { ReactTable, RowData } from "@tanstack/react-table";
import { TableHeaderGroup } from "./TableHeaderGroup";
import type { appTableFeaturesType } from "./tableFeatures";

interface MainTableProps<TValue extends RowData> {
	table: ReactTable<appTableFeaturesType, TValue>;
}

export default function MainTable<TValue extends RowData>({ table }: MainTableProps<TValue>) {
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
