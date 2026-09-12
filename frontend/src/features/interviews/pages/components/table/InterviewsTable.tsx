import { useTable } from "@tanstack/react-table";
import type { InterviewProjection } from "@/api/models";
import MainTable from "@/components/table/MainTable";
import { appTableFeatures } from "@/components/table/tableFeatures";
import { getColumns } from "./InterviewsTableColumns";

interface InterviewsTableProps {
	items: InterviewProjection[];
	onEdit?: (user: InterviewProjection) => void;
	onDelete?: (user: InterviewProjection) => void;
}

export function InterviewsTable({ items, onEdit }: InterviewsTableProps) {
	const table = useTable(
		{
			features: appTableFeatures,
			columns: getColumns(onEdit),
			data: items,
			getRowId: (item) => item.id,
			enableSorting: false,
		},
		(state) => ({
			sorting: state.sorting,
		}),
	);

	return <MainTable table={table} className="table-wide" />;
}
