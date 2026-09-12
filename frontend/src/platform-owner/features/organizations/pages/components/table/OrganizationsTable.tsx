import { useTable } from "@tanstack/react-table";
import { useRef } from "react";
import type { OrganizationProjection } from "@/api/models";
import MainTable from "@/components/table/MainTable";
import { appTableFeatures } from "@/components/table/tableFeatures";
import type { CreateOrganizationUserFormCommand } from "@/platform-owner/features/users/pages/components";
import CreateOrganizationUserDrawer from "@/platform-owner/features/users/pages/components/form/CreateOrganizationUserDrawer";
import { type Actions, getColumns } from "./OrganizationsTableColumns";

interface OrganizationsTableProps {
	items: OrganizationProjection[];
}

export function OrganizationsTable({ items }: OrganizationsTableProps) {
	const userFormRef = useRef<CreateOrganizationUserFormCommand>(null);
	const handleActions: Actions = {
		onAddUser: (it) => {
			userFormRef.current?.create(it.id, it.name);
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
			<MainTable table={table} />
			<CreateOrganizationUserDrawer ref={userFormRef} onSuccess={() => {}} />
		</>
	);
}
