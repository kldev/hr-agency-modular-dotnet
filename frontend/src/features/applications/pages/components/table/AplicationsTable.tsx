import { useTable } from "@tanstack/react-table";
import { useRef } from "react";
import type { JobApplicationProjection } from "@/api/models";
import MainTable from "@/components/table/MainTable";
import { appTableFeatures } from "@/components/table/tableFeatures";
import type {
	AddJobApplicationNoteFormCommand,
	ChangeJobApplicationStatusFormCommand,
} from "../forms";
import AddJobApplicationNoteDrawer from "../forms/AddJobApplicationNoteDrawer";
import ChangeJobApplicationStatusDrawer from "../forms/ChangeJobApplicationStatusDrawer";
import { type Actions, getColumns } from "./AplicationsTableColumns";

interface AplicationsTableProps {
	items: JobApplicationProjection[];
	onRefresh: () => void;
}

export function AplicationsTable({ items, onRefresh }: AplicationsTableProps) {
	const changeStatusRef = useRef<ChangeJobApplicationStatusFormCommand>(null);
	const addNoteRef = useRef<AddJobApplicationNoteFormCommand>(null);

	const handleActions: Actions = {
		addNote: (it) => {
			addNoteRef.current?.addNote(it.id);
		},
		addTag: (it) => {
			console.log(`Add tag: ${it.applicantEmail}`);
		},
		onChangeStatus: (it) => {
			changeStatusRef.current?.changeStatus(it.id, it.status);
		},
		onEdit: (it) => {
			console.log(`Edit ${it.applicantEmail}`);
		},
	};

	const table = useTable(
		{
			features: appTableFeatures,
			columns: getColumns(handleActions),
			data: items,
			getRowId: (user) => user.id,
			enableSorting: false,
		},
		(state) => ({
			sorting: state.sorting,
		}),
	);

	return (
		<>
			<MainTable table={table} className="table-wide" />
			<AddJobApplicationNoteDrawer ref={addNoteRef} onSuccess={onRefresh} />
			<ChangeJobApplicationStatusDrawer ref={changeStatusRef} onSuccess={onRefresh} />
		</>
	);
}
