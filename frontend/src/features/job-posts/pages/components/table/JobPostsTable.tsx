import { useTable } from "@tanstack/react-table";
import type { JobPostResponse } from "@/api/models";
import MainTable from "@/components/table/MainTable";
import { appTableFeatures } from "@/components/table/tableFeatures";
import { type Actions, getColumns } from "./JobPostsTableColumns";

interface JobPostsTableProps {
	items: JobPostResponse[];
	onRefresh?: () => void;
}

export function JobPostsTable({ items }: JobPostsTableProps) {
	const actionsHandler: Actions = {
		onAddApplication: () => { },
	};

	const table = useTable(
		{
			features: appTableFeatures,
			columns: getColumns(actionsHandler),
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
