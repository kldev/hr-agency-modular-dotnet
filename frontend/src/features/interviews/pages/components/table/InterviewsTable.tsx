import { useTable } from "@tanstack/react-table";
import { useRef } from "react";
import type { InterviewProjection } from "@/api/models";
import MainTable from "@/components/table/MainTable";
import { appTableFeatures } from "@/components/table/tableFeatures";
import {
	type ChangeInterviewerCommand,
	ChangeInterviewerDrawer,
	type ChangeInterviewFormatCommand,
	ChangeInterviewFormatDrawer,
	type ChangeInterviewStatusCommand,
	ChangeInterviewStatusDrawer,
} from "../forms";
import { type Actions, getColumns } from "./InterviewsTableColumns";

interface InterviewsTableProps {
	items: InterviewProjection[];
	onRefresh: () => void;
}

export function InterviewsTable({ items, onRefresh }: InterviewsTableProps) {
	const interviewerRef = useRef<ChangeInterviewerCommand>(null);
	const statusRef = useRef<ChangeInterviewStatusCommand>(null);

	const formatRef = useRef<ChangeInterviewFormatCommand>(null);

	const actionsHandler: Actions = {
		onChangeForamt: (item: InterviewProjection): void => {
			formatRef.current?.changeFormat(item.id);
		},
		onChangeStatus: (item: InterviewProjection): void => {
			statusRef.current?.changeStatus(item.id);
		},
		onChangeInterviewer: (item: InterviewProjection): void => {
			interviewerRef.current?.changeInterviewer(item.id);
		},
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

	return (
		<>
			<MainTable table={table} className="table-wide" />
			<ChangeInterviewFormatDrawer ref={formatRef} onSuccess={onRefresh} />
			<ChangeInterviewerDrawer ref={interviewerRef} onSuccess={onRefresh} />
			<ChangeInterviewStatusDrawer ref={statusRef} onSuccess={onRefresh} />
		</>
	);
}
