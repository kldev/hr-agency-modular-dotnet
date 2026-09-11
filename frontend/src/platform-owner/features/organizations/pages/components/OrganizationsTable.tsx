import { useTable } from "@tanstack/react-table";
import type { OrganizationProjection } from "@/api/models";
import MainTable from "@/components/table/MainTable";
import { appTableFeatures } from "@/components/table/tableFeatures";
import { getColumns } from "./OrganizationsTableColumns";

interface OrganizationsTableProps {
	items: OrganizationProjection[];
	onEdit?: (item: OrganizationProjection) => void;
}

export function OrganizationsTable({ items, onEdit }: OrganizationsTableProps) {
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
