import { createColumnHelper } from "@tanstack/react-table";
import { MessagePreview } from "#/components/ui/MessagePreview";
import type { TeamProjection } from "@/api/models";
import type { appTableFeaturesType } from "@/components/table";
import { ItemMark } from "@/components/ui";
import { formatDateTime } from "@/utlis/dateUtils";
import { teamRoles } from "../../types";
import { TeamActions } from "./TeamActions";

const columnHelper = createColumnHelper<appTableFeaturesType, TeamProjection>();

type ColumnHandlers = {
	onRename: (team: TeamProjection) => void;
	onAddMember: (team: TeamProjection) => void;
};

export function getColumns({ onRename, onAddMember }: ColumnHandlers) {
	return columnHelper.columns([
		columnHelper.display({
			id: "actions",
			header: () => null,
			meta: {
				width: "xxs",
			},
			cell: ({ row }) => (
				<div className="table-cell-content">
					<TeamActions
						id={row.original.id}
						onRename={() => onRename(row.original)}
						onAddMember={() => onAddMember(row.original)}
					/>
				</div>
			),
		}),
		columnHelper.accessor("name", {
			header: "Team",
			meta: {
				width: "xl",
			},

			cell: ({ row, getValue }) => (
				<div className="table-cell-content w-87.5">
					<ItemMark name={row.original.name} />

					<div>
						<div className="data-name">{getValue()}</div>
					</div>
				</div>
			),
		}),

		columnHelper.accessor("members", {
			header: "Members",
			meta: {
				width: "xs",
			},
			cell: ({ getValue }) => <span className="table-number">{getValue().length}</span>,
		}),

		columnHelper.display({
			id: "roster",
			header: "Roster",
			meta: {
				width: "2xl",
			},
			cell: ({ row }) => {
				const members = row.original.members
					.map(
						(member) => `${member.user.fullname ?? member.user.email} (${teamRoles[member.role]})`,
					)
					.join(", ");
				return (
					<div className="flex flex-col gap-3">
						<span className="truncate">
							<MessagePreview message={members} />
						</span>
					</div>
				);
			},
		}),

		columnHelper.accessor("createdAt", {
			header: "Created at",
			cell: ({ getValue }) => <span className="table-number">{formatDateTime(getValue())}</span>,
		}),
	]);
}
