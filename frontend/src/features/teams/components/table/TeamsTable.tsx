import { useNavigate } from "@tanstack/react-router";
import { useTable } from "@tanstack/react-table";
import { useRef } from "react";
import type { TeamProjection } from "@/api/models";
import MainTable from "@/components/table/MainTable";
import { appTableFeatures } from "@/components/table/tableFeatures";
import RenameTeamDrawer from "../form/RenameTeamDrawer";
import type { RenameTeamFormCommand } from "../form/TeamFormCommand";
import AddMemberDrawer, { type AddMemberFormCommand } from "../members/AddMemberDrawer";
import { getColumns } from "./TeamsTableColumns";

interface TeamsTableProps {
	teams: TeamProjection[];
	onRefresh: () => void;
}

export function TeamsTable({ teams, onRefresh }: TeamsTableProps) {
	const navigate = useNavigate();
	const renameRef = useRef<RenameTeamFormCommand>(null);
	const addMemberRef = useRef<AddMemberFormCommand>(null);

	const table = useTable(
		{
			features: appTableFeatures,
			columns: getColumns({
				onRename: (team) => renameRef.current?.rename(team.id, team.name),
				onAddMember: (team) => addMemberRef.current?.add(team.id, team.name),
			}),
			data: teams,
			getRowId: (team) => team.id,
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
				className="table-wide"
				onRowClick={(team) => {
					navigate({ to: "/app/teams/$id", params: { id: team.id }, search: { search: "" } });
				}}
			/>

			<RenameTeamDrawer ref={renameRef} onSuccess={onRefresh} />
			<AddMemberDrawer ref={addMemberRef} onSuccess={onRefresh} />
		</>
	);
}
