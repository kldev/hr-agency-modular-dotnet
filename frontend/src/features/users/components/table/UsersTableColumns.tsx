import { createColumnHelper } from "@tanstack/react-table";
import type { OrgUnitRow, UserProjection } from "@/api/models";
import type { appTableFeaturesType } from "@/components/table";
import { ItemMark } from "@/components/ui";
import { teamRoles } from "@/features/teams/types";
import { formatDateTime } from "@/utlis/dateUtils";
import { organizationRoleLabel } from "../../types";
import { UserActions } from "./UserActions";

const columnHelper = createColumnHelper<appTableFeaturesType, UserProjection>();

export type UserColumnHandlers = {
	onEdit: (user: UserProjection) => void;
	onChangeRole: (user: UserProjection) => void;
	onChangeTeam: (user: UserProjection) => void;
	/*
	 * The unit is not on `UserProjection` the way the team is - the chart is never mirrored onto the
	 * person - so the page resolves it from the org structure and hands the lookup down.
	 */
	unitOf: (userId: string) => OrgUnitRow | undefined;
};

export function getColumns({ onEdit, onChangeRole, onChangeTeam, unitOf }: UserColumnHandlers) {
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
			cell: ({ getValue }) => organizationRoleLabel(getValue()),
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

		columnHelper.display({
			id: "unit",
			header: "Unit",
			meta: {
				width: "md",
			},
			cell: ({ row }) => {
				const unit = unitOf(row.original.id);

				if (!unit) {
					return <span className="text-(--color-text-muted)">—</span>;
				}

				return (
					<div className="table-cell-truncate" title={unit.name}>
						<div className="data-name">{unit.name}</div>

						{unit.headUserId === row.original.id ? (
							<div className="text-xs text-(--color-text-muted)">Heads it</div>
						) : null}
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
