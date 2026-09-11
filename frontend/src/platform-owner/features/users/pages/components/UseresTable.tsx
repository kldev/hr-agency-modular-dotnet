import { useTable } from "@tanstack/react-table";
import type { UserProjection } from "@/api/models";
import MainTable from "@/components/table/MainTable";
import { appTableFeatures } from "@/components/table/tableFeatures";
import { getColumns } from "./UsersTableColumns";

interface UseresTableProps {
	users: UserProjection[];
	onEdit?: (user: UserProjection) => void;
	onDelete?: (user: UserProjection) => void;
}

export function UseresTable({ users, onEdit }: UseresTableProps) {
	const table = useTable(
		{
			features: appTableFeatures,
			columns: getColumns(onEdit),
			data: users,
			getRowId: (user) => user.id,
			enableSorting: false,
		},
		(state) => ({
			sorting: state.sorting,
		}),
	);

	return <MainTable table={table} />;
}
