import { useTable } from "@tanstack/react-table";
import { useRef } from "react";
import type { JobDescriptionProjection } from "@/api/models";
import MainTable from "@/components/table/MainTable";
import { appTableFeatures } from "@/components/table/tableFeatures";
import { type ChangeRecruiterCommand, ChangeRecruiterDrawer } from "../forms";
import { type Actions, getColumns } from "./JobsDescriptopnTableColumns";

interface JobsDescriptopnTableProps {
	items: JobDescriptionProjection[];
	onRefresh?: () => void;
}

export function JobsDescriptopnTable({ items, onRefresh }: JobsDescriptopnTableProps) {
	const recruiterRef = useRef<ChangeRecruiterCommand>(null);

	const actionsHandler: Actions = {
		onChangeStatus: () => {},

		onChangeRecruiter: (item) => {
			recruiterRef.current?.changeRecruiter(item.id, item.recruiterId);
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

			<ChangeRecruiterDrawer
				ref={recruiterRef}
				onSuccess={() => {
					onRefresh?.();
				}}
			/>
		</>
	);
}
