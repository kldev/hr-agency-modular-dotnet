import { useTable } from "@tanstack/react-table";
import type { ProjectProjection } from "@/api/models";
import MainTable from "@/components/table/MainTable";
import { appTableFeatures } from "@/components/table/tableFeatures";
import { getColumns } from "./ProjectsTableColumns";

interface ProjectsTableProps {
	projects: ProjectProjection[];
	onEdit: (project: ProjectProjection) => void;
	onChangeStatus: (project: ProjectProjection) => void;
}

/**
 * The wizard and the drawers belong to the page, not here: the table and the card list are two
 * views of one list, and one copy each means the same project can be open in two of them at once.
 */
export function ProjectsTable({ projects, onEdit, onChangeStatus }: ProjectsTableProps) {
	const table = useTable(
		{
			features: appTableFeatures,
			columns: getColumns({ onEdit, onChangeStatus }),
			data: projects,
			getRowId: (project) => project.id,
			enableSorting: false,
		},
		(state) => ({
			sorting: state.sorting,
		}),
	);

	return <MainTable table={table} className="table-wide" />;
}
