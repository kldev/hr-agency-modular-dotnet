import { type SortDescriptor, Table } from "@heroui/react";
import type { ReactTable, RowData, SortingState } from "@tanstack/react-table";
import { TableHeaderCell } from "./TableHeaderCell";
import type { appTableFeaturesType } from "./tableFeatures";

interface MainTableProps<TValue extends RowData> {
	table: ReactTable<appTableFeaturesType, TValue>;
	className?: string;
	onRowClick?: (value: TValue) => void;
	/** Accessible name of the table; the lists have a visible page title, so this has a default. */
	"aria-label"?: string;
}

const toSortDescriptor = (sorting: SortingState | undefined): SortDescriptor | undefined =>
	sorting?.[0]
		? { column: sorting[0].id, direction: sorting[0].desc ? "descending" : "ascending" }
		: undefined;

const toSortingState = (descriptor: SortDescriptor): SortingState => [
	{ id: String(descriptor.column), desc: descriptor.direction === "descending" },
];

/**
 * A TanStack table rendered by HeroUI: columns, row model and sorting state come from TanStack,
 * markup and keyboard navigation from React Aria. Empty state, loading and "load more" stay with
 * the page (`Page`, `LoadMore`), as they did before - the table only draws the rows it is given.
 */
export default function MainTable<TValue extends RowData>({
	table,
	className,
	onRowClick,
	"aria-label": ariaLabel = "Data table",
}: MainTableProps<TValue>) {
	// React Aria has one header row; the leaf group is the one whose columns own the cells.
	const headers = table.getHeaderGroups().at(-1)?.headers ?? [];

	return (
		<Table variant="secondary" className="table-container">
			<Table.ScrollContainer>
				<Table.Content
					aria-label={ariaLabel}
					className={className}
					sortDescriptor={toSortDescriptor(table.state.sorting)}
					onSortChange={(descriptor) => table.setSorting(toSortingState(descriptor))}
				>
					<Table.Header>
						{headers.map((header, index) => {
							const width = header.column.columnDef.meta?.width;

							return (
								<Table.Column
									key={header.id}
									id={header.column.id}
									isRowHeader={index === 0}
									allowsSorting={!header.isPlaceholder && header.column.getCanSort()}
									className={width ? `table-header-${width}` : undefined}
								>
									<TableHeaderCell header={header} table={table} />
								</Table.Column>
							);
						})}
					</Table.Header>

					<Table.Body>
						{table.getRowModel().rows.map((row) => (
							<Table.Row
								key={row.id}
								id={row.id}
								data-id={row.id}
								// A double click, not React Aria's `onAction`: a single press would also fire
								// from the action menu and links inside the row, and would get in the way of
								// selecting text in a cell.
								onDoubleClick={onRowClick ? () => onRowClick(row.original) : undefined}
							>
								{row.getAllCells().map((cell) => {
									const meta = cell.column.columnDef.meta;

									return (
										<Table.Cell
											key={cell.id}
											data-id={cell.id}
											className={[
												meta?.align === "right" ? "table-number" : "",
												meta?.width ? `cell-${meta.width}` : "",
												meta?.className ?? "",
											]
												.filter(Boolean)
												.join(" ")}
										>
											<table.FlexRender cell={cell} />
										</Table.Cell>
									);
								})}
							</Table.Row>
						))}
					</Table.Body>
				</Table.Content>
			</Table.ScrollContainer>
		</Table>
	);
}
