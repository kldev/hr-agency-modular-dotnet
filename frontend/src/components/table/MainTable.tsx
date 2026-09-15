import type { ReactTable, RowData } from "@tanstack/react-table";
import { TableHeaderGroup } from "./TableHeaderGroup";
import type { appTableFeaturesType } from "./tableFeatures";

interface MainTableProps<TValue extends RowData> {
	table: ReactTable<appTableFeaturesType, TValue>;
	className?: string;
	onRowClick?: (value: TValue) => void;
}

export default function MainTable<TValue extends RowData>({
	table,
	className,
	onRowClick,
}: MainTableProps<TValue>) {
	return (
		<div className="table-container">
			<table className={["table", className].join(" ")}>
				<thead>
					{table.getHeaderGroups().map((headerGroup) => (
						<TableHeaderGroup key={headerGroup.id} table={table} group={headerGroup} />
					))}
				</thead>

				<tbody>
					{table.getRowModel().rows.map((row) => {
						return (
							<tr
								key={row.id}
								data-id={row.id}
								onDoubleClick={() => {
									onRowClick?.(row.original);
								}}
							>
								{row.getAllCells().map((cell) => {
									const meta = cell.column.columnDef.meta;
									const className =
										meta?.align === "right"
											? `table-number ${meta?.className}`
											: `${meta?.className}`;
									const width = meta?.width;

									const cellClass = width ? `cell-${width.toString()}` : "";
									return (
										<td
											key={cell.id}
											data-id={cell.id}
											className={[className, cellClass].join(" ")}
										>
											<table.FlexRender cell={cell} />
										</td>
									);
								})}
							</tr>
						);
					})}
				</tbody>
			</table>
		</div>
	);
}
