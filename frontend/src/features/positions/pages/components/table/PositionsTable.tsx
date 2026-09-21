import { useTable } from "@tanstack/react-table";
import type { PositionListItem } from "@/api/models";
import MainTable from "@/components/table/MainTable";
import { appTableFeatures } from "@/components/table/tableFeatures";
import { getColumns } from "./PositionsTableColumns";

interface PositionsTableProps {
	positions: PositionListItem[];
	onEdit: (position: PositionListItem) => void;
	onArchive: (position: PositionListItem) => void;
	onRestore: (position: PositionListItem) => void;
}

/** The dialogs belong to the page: the table and the card list are two views of one list. */
export function PositionsTable({ positions, onEdit, onArchive, onRestore }: PositionsTableProps) {
	const table = useTable(
		{
			features: appTableFeatures,
			columns: getColumns({ onEdit, onArchive, onRestore }),
			data: positions,
			getRowId: (position) => position.id,
			enableSorting: false,
		},
		(state) => ({
			sorting: state.sorting,
		}),
	);

	return <MainTable table={table} className="table-wide" />;
}
