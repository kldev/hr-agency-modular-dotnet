import type { Header_Core, ReactTable, RowData } from "@tanstack/react-table";
import { ArrowDown, ArrowUp, ArrowUpDown } from "lucide-react";
import type { appTableFeaturesType } from "./tableFeatures";

interface TableHeaderCellProps<TValue extends RowData> {
	header: Header_Core<appTableFeaturesType, TValue>;
	table: ReactTable<appTableFeaturesType, TValue>;
}

/**
 * What goes inside a `Table.Column`. The column itself is the sort control (React Aria makes a
 * sortable header pressable and focusable), so there is no button in here - only the label and,
 * for a sortable column, the direction it is sorted in.
 */
export function TableHeaderCell<TValue extends RowData>({
	header,
	table,
}: TableHeaderCellProps<TValue>) {
	if (header.isPlaceholder) {
		return null;
	}

	const { column } = header;

	if (!column.getCanSort()) {
		return (
			<span className="table-header-label">
				<table.FlexRender header={header} />
			</span>
		);
	}

	const sorted = column.getIsSorted();

	return (
		<span className="table-sort-label">
			<table.FlexRender header={header} />

			{sorted === "asc" && <ArrowUp className="size-3.5" />}

			{sorted === "desc" && <ArrowDown className="size-3.5" />}

			{!sorted && <ArrowUpDown className="size-3.5 opacity-40" />}
		</span>
	);
}
