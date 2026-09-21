import { useTable } from "@tanstack/react-table";
import type { WorkerProjection } from "@/api/models";
import MainTable from "@/components/table/MainTable";
import { appTableFeatures } from "@/components/table/tableFeatures";
import { getColumns } from "./WorkersTableColumns";

interface WorkersTableProps {
	workers: WorkerProjection[];
	onEdit?: (worker: WorkerProjection) => void;
	onChangeStatus?: (worker: WorkerProjection) => void;
	onPlanAssignment?: (worker: WorkerProjection) => void;
}

/**
 * Unlike the projects table this one owns no drawers. The page does, and hands the callbacks down
 * to both the table and the card list - otherwise each of the two mounts its own copy of every
 * drawer and the same worker can be open in two of them at once.
 */
export function WorkersTable({
	workers,
	onEdit,
	onChangeStatus,
	onPlanAssignment,
}: WorkersTableProps) {
	const table = useTable(
		{
			features: appTableFeatures,
			columns: getColumns({ onEdit, onChangeStatus, onPlanAssignment }),
			data: workers,
			getRowId: (worker) => worker.id ?? "",
			enableSorting: false,
		},
		(state) => ({
			sorting: state.sorting,
		}),
	);

	return <MainTable table={table} className="table-wide" />;
}
