import { useTable } from "@tanstack/react-table";
import { useRef } from "react";
import type { ProjectProjection } from "@/api/models";
import MainTable from "@/components/table/MainTable";
import { appTableFeatures } from "@/components/table/tableFeatures";
import {
	ChangeProjectStatusDrawer,
	type ChangeProjectStatusFormCommand,
	EditProjectDrawer,
	type EditProjectFormCommand,
} from "../../../drawers";
import { getColumns } from "./ProjectsTableColumns";

interface ProjectsTableProps {
	projects: ProjectProjection[];
	onRefresh: () => void;
}

export function ProjectsTable({ projects, onRefresh }: ProjectsTableProps) {
	const editRef = useRef<EditProjectFormCommand>(null);
	const statusRef = useRef<ChangeProjectStatusFormCommand>(null);

	const table = useTable(
		{
			features: appTableFeatures,
			columns: getColumns({
				onEdit: (project) => {
					editRef.current?.edit(project);
				},
				onChangeStatus: (project) => {
					statusRef.current?.changeStatus(project);
				},
			}),
			data: projects,
			getRowId: (project) => project.id,
			enableSorting: false,
		},
		(state) => ({
			sorting: state.sorting,
		}),
	);

	return (
		<>
			<MainTable table={table} className="table-wide" />

			<EditProjectDrawer ref={editRef} onSuccess={onRefresh} />
			<ChangeProjectStatusDrawer ref={statusRef} onSuccess={onRefresh} />
		</>
	);
}
