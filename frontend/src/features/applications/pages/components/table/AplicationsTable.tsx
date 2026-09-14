import { useTable } from "@tanstack/react-table";
import { useRef } from "react";
import type { JobApplicationProjection } from "@/api/models";
import MainTable from "@/components/table/MainTable";
import { appTableFeatures } from "@/components/table/tableFeatures";
import { ApplicationsActionDrawers, type JobApplicationsRef } from "../forms";
import { type Actions, getColumns } from "./AplicationsTableColumns";

interface AplicationsTableProps {
	items: JobApplicationProjection[];
	onRefresh: () => void;
}

export function AplicationsTable({ items, onRefresh }: AplicationsTableProps) {
	const formRef = useRef<JobApplicationsRef>(null);

	const handleActions: Actions = {
		onAction: (action, item) => {
			formRef.current?.update(item.id, action, item.status);
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
			<ApplicationsActionDrawers ref={formRef} onSuccess={onRefresh} />
		</>
	);
}
