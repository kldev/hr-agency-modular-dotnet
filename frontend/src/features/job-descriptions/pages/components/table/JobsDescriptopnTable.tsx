import { useTable } from "@tanstack/react-table";
import type { JobDescriptionProjection } from "@/api/models";
import MainTable from "@/components/table/MainTable";
import { appTableFeatures } from "@/components/table/tableFeatures";
import { type Actions, getColumns } from "./JobsDescriptopnTableColumns";

interface JobsDescriptopnTableProps {
	items: JobDescriptionProjection[];
	onRefresh?: () => void;
}

export function JobsDescriptopnTable({ items }: JobsDescriptopnTableProps) {
	const actionsHandler: Actions = {
		onChangeStatus: () => {},
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
