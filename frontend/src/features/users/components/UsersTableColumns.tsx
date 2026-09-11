import { createColumnHelper } from "@tanstack/react-table";
import { MoreHorizontal } from "lucide-react";
import type { UserProjection } from "@/api/models";
import type { appTableFeaturesType } from "@/components/table";
import { formatDateTime } from "@/utlis/dateUtils";

const columnHelper = createColumnHelper<appTableFeaturesType, UserProjection>();

function UserMark({ user }: { user: UserProjection }) {
	const initials =
		user.fullName ?
			user.fullName
				.split(" ")
				.slice(0, 2)
				.map((part) => part[0])
				.join("")
				.toUpperCase() : "";

	return <span className="data-avatar">{initials}</span>;
}

export function getColumns(onEdit?: (company: UserProjection) => void) {
	const columns = columnHelper.columns([
		columnHelper.accessor("fullName", {
			header: "User",
			meta: {
				width: "2xl",
			},

			cell: ({ row, getValue }) => (
				<div className="table-cell-content w-87.5">
					<UserMark user={row.original} />

					<div>
						<div className="data-name">{getValue()}</div>

						<div className="data-meta max-w-87.5 truncate">{row.original.email}</div>
					</div>
				</div>
			),
		}),

		columnHelper.accessor("role", {
			header: "Role",
		}),

		columnHelper.accessor("createdAt", {
			header: "Created at",
			cell: ({ getValue }) => <span className="table-number">{formatDateTime(getValue())}</span>,
		}),

		columnHelper.display({
			id: "actions",
			header: () => null,
			cell: ({ row }) => {
				const user = row.original;

				if (!onEdit) {
					return null;
				}

				return (
					<div className="table-actions">
						<button
							type="button"
							className="table-action-button"
							aria-label={`Actions for ${user.email}`}
						>
							<MoreHorizontal className="size-4" />
						</button>
					</div>
				);
			},
		}),
	]);
	return columns;
}
