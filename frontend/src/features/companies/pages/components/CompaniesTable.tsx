import { useTable } from "@tanstack/react-table";
import type { CompanyProjection } from "@/api/models";
import MainTable from "@/components/table/MainTable";
import { appTableFeatures } from "@/components/table/tableFeatures";
import { getColumns } from "./CompaniesTableColumns";

interface CompaniesTableProps {
	companies: CompanyProjection[];
	onEdit?: (company: CompanyProjection) => void;
	onDelete?: (company: CompanyProjection) => void;
}

export function CompaniesTable({ companies, onEdit }: CompaniesTableProps) {
	const table = useTable(
		{
			features: appTableFeatures,
			columns: getColumns(onEdit),
			data: companies,
			getRowId: (company) => company.id,
			enableSorting: false,
		},
		(state) => ({
			sorting: state.sorting,
		}),
	);

	return <MainTable table={table} className="table-wide" />;
}
