import { useTable } from "@tanstack/react-table";
import type { OpportunityProjection } from "@/api/models";
import MainTable from "@/components/table/MainTable";
import { appTableFeatures } from "@/components/table/tableFeatures";
import { getColumns } from "./SalesTableColumns";

interface SalesTableProps {
	items: OpportunityProjection[];
	onEdit?: (item: OpportunityProjection) => void;
	onDelete?: (item: OpportunityProjection) => void;
}

export function SalesTable({ items, onEdit }: SalesTableProps) {
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

	return <MainTable table={table} className="table-wide" />;
}
