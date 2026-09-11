import type { Header_Core, ReactTable, RowData } from "@tanstack/react-table";

import { ArrowDown, ArrowUp, ArrowUpDown } from "lucide-react";
import type { appTableFeaturesType } from "./tableFeatures";

///const table: ReactTable<appTableFeaturesType, CompanyProjection, {

// table-number
interface TableHeaderCellProps<TValue extends RowData> {
	header: Header_Core<appTableFeaturesType, TValue>;
	table: ReactTable<appTableFeaturesType, TValue>;
}

export function TableHeaderCell<TValue extends RowData>({
	header,
	table,
}: TableHeaderCellProps<TValue>) {
	if (header.isPlaceholder) {
		return <th />;
	}

	const { column } = header;
	const sorted = column.getIsSorted();
	const width = column.columnDef.meta?.width;

	const cellClass = width ? `table-header-${width.toString()}` : "";

	return (
		<th scope="col" className={cellClass}>
			{column.getCanSort() ? (
				<button
					type="button"
					className="table-sort-button"
					onClick={column.getToggleSortingHandler()}
					aria-label={`Sort by ${String(column.columnDef.header)}`}
				>
					<table.FlexRender header={header} />

					{sorted === "asc" && <ArrowUp className="size-3.5" />}

					{sorted === "desc" && <ArrowDown className="size-3.5" />}

					{!sorted && <ArrowUpDown className="size-3.5 opacity-40" />}
				</button>
			) : (
				<table.FlexRender header={header} />
			)}
		</th>
	);
}
