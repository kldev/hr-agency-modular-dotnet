import { useTable } from "@tanstack/react-table";
import { useRef } from "react";
import type { CandidateProjection } from "@/api/models";
import MainTable from "@/components/table/MainTable";
import { appTableFeatures } from "@/components/table/tableFeatures";
import { EditCandidateDrawer, type EditCandidateFormCommand } from "../form";
import { getColumns } from "./CandidatesTableColumns";

interface CandidatesTableProps {
	items: CandidateProjection[];
	onRefresh: () => void;
}

export function CandidatesTable({ items, onRefresh }: CandidatesTableProps) {
	const formRef = useRef<EditCandidateFormCommand>(null);

	const handleOnEdit = (item: CandidateProjection) => {
		formRef.current?.edit(item.id);
	};

	const table = useTable(
		{
			features: appTableFeatures,
			columns: getColumns(handleOnEdit),
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
			<MainTable table={table} />
			<EditCandidateDrawer ref={formRef} onSuccess={onRefresh} />
		</>
	);
}
