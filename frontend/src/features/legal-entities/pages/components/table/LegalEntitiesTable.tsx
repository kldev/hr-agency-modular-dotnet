import { useNavigate } from "@tanstack/react-router";
import { useTable } from "@tanstack/react-table";
import type { LegalEntityProjection } from "@/api/models";
import MainTable from "@/components/table/MainTable";
import { appTableFeatures } from "@/components/table/tableFeatures";
import { getColumns } from "./LegalEntitiesTableColumns";

interface LegalEntitiesTableProps {
	entities: LegalEntityProjection[];
	onEdit: (entity: LegalEntityProjection) => void;
	onClose: (entity: LegalEntityProjection) => void;
}

export function LegalEntitiesTable({ entities, onEdit, onClose }: LegalEntitiesTableProps) {
	const navigate = useNavigate();

	const table = useTable(
		{
			features: appTableFeatures,
			columns: getColumns({ onEdit, onClose }),
			data: entities,
			getRowId: (entity) => entity.id,
			enableSorting: false,
		},
		(state) => ({ sorting: state.sorting }),
	);

	return (
		<MainTable
			table={table}
			className="table-wide"
			onRowClick={(entity) => {
				navigate({ to: "/app/legal-entities/$id", params: { id: entity.id } });
			}}
		/>
	);
}
