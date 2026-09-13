import { useTable } from "@tanstack/react-table";
import { useRef } from "react";
import type { InterviewProjection } from "@/api/models";
import MainTable from "@/components/table/MainTable";
import { appTableFeatures } from "@/components/table/tableFeatures";
import {
	InterviewActionDrawers,
	type InterviewActionsRef,
	type InterviewActionsType,
} from "../forms";

import { type Actions, getColumns } from "./InterviewsTableColumns";

interface InterviewsTableProps {
	items: InterviewProjection[];
	onRefresh: () => void;
}

export function InterviewsTable({ items, onRefresh }: InterviewsTableProps) {
	const updateRef = useRef<InterviewActionsRef>(null);

	const actionsHandler: Actions = {
		onAction: (action: InterviewActionsType, item: InterviewProjection): void => {
			updateRef?.current?.update(item.id, action);
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
			<InterviewActionDrawers ref={updateRef} onSuccess={onRefresh} />
		</>
	);
}
