import { useTable } from "@tanstack/react-table";
import type { AssignmentProjection } from "@/api/models";
import MainTable from "@/components/table/MainTable";
import { appTableFeatures } from "@/components/table/tableFeatures";
import { getColumns } from "./AssignmentsTableColumns";

interface AssignmentsTableProps {
	assignments: AssignmentProjection[];
	onEdit?: (assignment: AssignmentProjection) => void;
	onChangeStatus?: (assignment: AssignmentProjection) => void;
}

/**
 * The drawers belong to the page, not here: the table and the card list are two views of one list,
 * and a drawer owned by each of them is the same record open twice.
 */
export function AssignmentsTable({ assignments, onEdit, onChangeStatus }: AssignmentsTableProps) {
	const table = useTable(
		{
			features: appTableFeatures,
			columns: getColumns({ onEdit, onChangeStatus }),
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
