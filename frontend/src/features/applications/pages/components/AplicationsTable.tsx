import { useTable } from "@tanstack/react-table";
import type { JobApplicationProjection } from "@/api/models";
import MainTable from "@/components/table/MainTable";
import { appTableFeatures } from "@/components/table/tableFeatures";
import { getColumns } from "./AplicationsTableColumns";

interface AplicationsTableProps {
	items: JobApplicationProjection[];
	onEdit?: (user: JobApplicationProjection) => void;
	onDelete?: (user: JobApplicationProjection) => void;
}

export function AplicationsTable({ items, onEdit }: AplicationsTableProps) {
	const table = useTable(
		{
			features: appTableFeatures,
			columns: getColumns(onEdit),
			data: items,
			getRowId: (user) => user.id,
			enableSorting: false,
		},
		(state) => ({
			sorting: state.sorting,
		}),
	);

	return <MainTable table={table} />;
}
