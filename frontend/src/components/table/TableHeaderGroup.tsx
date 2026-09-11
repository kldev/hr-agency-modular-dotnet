import type { HeaderGroup, ReactTable, RowData } from "@tanstack/react-table";

import { TableHeaderCell } from "./TableHeaderCell";
import type { appTableFeaturesType } from "./tableFeatures";

interface TableHeaderGroupProps<TValue extends RowData> {
	group: HeaderGroup<appTableFeaturesType, TValue>;
	table: ReactTable<appTableFeaturesType, TValue>;
}

export function TableHeaderGroup<TData extends RowData>({
	group,
	table,
}: TableHeaderGroupProps<TData>) {
	return (
		<tr key={group.id}>
			{group.headers.map((header) => (
				<TableHeaderCell key={header.id} header={header} table={table} />
			))}
		</tr>
	);
}
