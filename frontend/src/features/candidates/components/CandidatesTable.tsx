import { useTable } from "@tanstack/react-table";
import type { CandidateProjection } from "@/api/models";
import MainTable from "@/components/table/MainTable";
import { appTableFeatures } from "@/components/table/tableFeatures";
import { getColumns } from "./CandidatesTableColumns";

interface CandidatesTableProps {
	items: CandidateProjection[];
	onEdit?: (user: CandidateProjection) => void;
	onDelete?: (user: CandidateProjection) => void;
}

export function CandidatesTable({ items, onEdit }: CandidatesTableProps) {
	const table = useTable(
		{
			features: appTableFeatures,
			columns: getColumns(onEdit),
			data: items,
			getRowId: (user) => user.id,
			enableSorting: false,
		},
		(state) => ({
			sorting: state.sorting,
		}),
	);

	return <MainTable table={table} />;
}
