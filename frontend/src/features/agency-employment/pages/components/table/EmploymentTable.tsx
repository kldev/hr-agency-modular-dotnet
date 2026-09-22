import { useTable } from "@tanstack/react-table";
import type { AgencyEmploymentProjection } from "@/api/models";
import MainTable from "@/components/table/MainTable";
import { appTableFeatures } from "@/components/table/tableFeatures";
import { getColumns } from "./EmploymentTableColumns";

interface Props {
	employments: AgencyEmploymentProjection[];
	onChangeTerms: (employment: AgencyEmploymentProjection) => void;
	onEnd: (employment: AgencyEmploymentProjection) => void;
}

export function EmploymentTable({ employments, onChangeTerms, onEnd }: Props) {
	const table = useTable(
		{
			features: appTableFeatures,
			columns: getColumns({ onChangeTerms, onEnd }),
			data: employments,
			getRowId: (employment) => employment.id,
			enableSorting: false,
		},
		(state) => ({ sorting: state.sorting }),
	);

	return <MainTable table={table} className="table-wide" />;
}
