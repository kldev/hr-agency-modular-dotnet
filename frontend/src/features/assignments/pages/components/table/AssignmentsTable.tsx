import { useTable } from "@tanstack/react-table";
import type { AssignmentProjection } from "@/api/models";
import MainTable from "@/components/table/MainTable";
import { appTableFeatures } from "@/components/table/tableFeatures";
import { getColumns } from "./AssignmentsTableColumns";

interface AssignmentsTableProps {
	assignments: AssignmentProjection[];
}

export function AssignmentsTable({ assignments }: AssignmentsTableProps) {
	const table = useTable(
		{
			features: appTableFeatures,
			columns: getColumns(),
			data: assignments,
			getRowId: (assignment) => assignment.id,
			enableSorting: false,
		},
		(state) => ({
			sorting: state.sorting,
		}),
	);

	return <MainTable table={table} className="table-wide" />;
}
