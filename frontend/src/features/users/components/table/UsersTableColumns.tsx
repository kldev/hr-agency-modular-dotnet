import { createColumnHelper } from "@tanstack/react-table";
import type { UserProjection } from "@/api/models";
import type { appTableFeaturesType } from "@/components/table";
import { ItemMark } from "@/components/ui";
import { teamRoles } from "@/features/teams/types";
import { formatDateTime } from "@/utlis/dateUtils";
import { UserActions } from "./UserActions";

const columnHelper = createColumnHelper<appTableFeaturesType, UserProjection>();

export type UserColumnHandlers = {
	onEdit: (user: UserProjection) => void;
	onChangeRole: (user: UserProjection) => void;
	onChangeTeam: (user: UserProjection) => void;
};

export function getColumns({ onEdit, onChangeRole, onChangeTeam }: UserColumnHandlers) {
	const columns = columnHelper.columns([
		columnHelper.display({
			id: "actions",
			meta: {
				width: "xxs",
			},
			header: () => null,
			cell: ({ row }) => (
				<UserActions
					id={row.original.id}
					email={row.original.email}
					onEdit={() => onEdit(row.original)}
					onChangeRole={() => onChangeRole(row.original)}
					onChangeTeam={() => onChangeTeam(row.original)}
				/>
			),
		}),
		columnHelper.accessor("fullName", {
			header: "User",
			meta: {
				width: "2xl",
			},

			cell: ({ row, getValue }) => (
				<div className="table-cell-content w-87.5">
					<ItemMark name={row.original.fullName} />

					<div>
						<div className="data-name">{getValue()}</div>
					</div>
				</div>
			),
		}),
		columnHelper.accessor("email", {
			header: "Email",
			cell: ({ getValue }) => <span className="table-number truncate">{getValue()}</span>,
			meta: {
				width: "xl",
			},
		}),
		columnHelper.accessor("role", {
			header: "Role",
			meta: {
				width: "sm",
			},
		}),

		columnHelper.display({
			id: "team",
			header: "Team",
			meta: {
				width: "md",
			},
			cell: ({ row }) => {
				const team = row.original.team;

				if (!team) {
					return <span className="text-(--color-text-muted)">—</span>;
				}

				return (
					<div>
						<div className="data-name">{team.name}</div>
						<div className="text-xs text-(--color-text-muted)">{teamRoles[team.role]}</div>
					</div>
				);
			},
		}),

		columnHelper.accessor("phone", {
			header: "Phone",
			meta: {
				width: "md",
			},
		}),

		columnHelper.accessor("createdAt", {
			header: "Created at",
			meta: {
				width: "md",
			},
			cell: ({ getValue }) => <span className="table-number">{formatDateTime(getValue())}</span>,
		}),
	]);

	return columns;
}
