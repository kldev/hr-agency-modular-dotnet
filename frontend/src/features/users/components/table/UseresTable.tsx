import { useNavigate } from "@tanstack/react-router";
import { useTable } from "@tanstack/react-table";
import { useRef } from "react";
import type { UserProjection } from "@/api/models";
import MainTable from "@/components/table/MainTable";
import { appTableFeatures } from "@/components/table/tableFeatures";
import {
	ChangeUserRoleDrawer,
	type ChangeUserRoleFormCommand,
	ChangeUserTeamDrawer,
	type ChangeUserTeamFormCommand,
	EditUserDrawer,
	type EditUserFormCommand,
} from "../form";
import { getColumns } from "./UsersTableColumns";

interface UseresTableProps {
	users: UserProjection[];
	onRefresh: () => void;
}

export function UseresTable({ users, onRefresh }: UseresTableProps) {
	const navigate = useNavigate();
	const editRef = useRef<EditUserFormCommand>(null);
	const roleRef = useRef<ChangeUserRoleFormCommand>(null);
	const teamRef = useRef<ChangeUserTeamFormCommand>(null);

	const table = useTable(
		{
			features: appTableFeatures,
			columns: getColumns({
				onEdit: (user) => editRef.current?.edit(user),
				onChangeRole: (user) => roleRef.current?.changeRole(user),
				onChangeTeam: (user) => teamRef.current?.changeTeam(user),
			}),
			data: users,
			getRowId: (user) => user.id,
			enableSorting: false,
		},
		(state) => ({
			sorting: state.sorting,
		}),
	);

	return (
		<>
			<MainTable
				table={table}
				onRowClick={(user) => {
					navigate({ to: "/app/users/$id", params: { id: user.id }, search: {} });
				}}
			/>

			<EditUserDrawer ref={editRef} onSuccess={onRefresh} />
			<ChangeUserRoleDrawer ref={roleRef} onSuccess={onRefresh} />
			<ChangeUserTeamDrawer ref={teamRef} onSuccess={onRefresh} />
		</>
	);
}
