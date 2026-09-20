import { Link } from "@tanstack/react-router";
import { createColumnHelper } from "@tanstack/react-table";
import type { ProjectProjection } from "@/api/models";
import type { appTableFeaturesType } from "@/components/table";
import { ItemMark, ProjectStatusBadge } from "@/components/ui";
import { engagementTypes } from "../../../types";
import { formatPeriod } from "../../../utils";
import { ComplianceChip } from "../ComplianceChip";
import { ProjectActions } from "./ProjectActions";

const columnHelper = createColumnHelper<appTableFeaturesType, ProjectProjection>();

type projectsActions = {
	onEdit: (project: ProjectProjection) => void;
	onChangeStatus: (project: ProjectProjection) => void;
};

export function getColumns(actions: projectsActions) {
	return columnHelper.columns([
		columnHelper.accessor("name", {
			header: "Project",
			meta: {
				width: "2xl",
			},

			cell: ({ row, getValue }) => {
				const project = row.original;

				return (
					<div className="table-cell-content">
						<ProjectActions
							id={project.id}
							onEdit={() => actions.onEdit(project)}
							onChangeStatus={() => actions.onChangeStatus(project)}
						/>

						<ItemMark name={project.name} />

						<div>
							<div className="data-name">
								<Link
									to="/app/projects/$id"
									params={{ id: project.id }}
									search={{ search: undefined }}
								>
									{getValue()}
								</Link>
							</div>

							<div className="data-meta max-w-67.5 truncate">{project.description}</div>
						</div>
					</div>
				);
			},
		}),

		columnHelper.accessor("companyName", {
			header: "Client",
		}),

		columnHelper.accessor("status", {
			header: "Status",
			meta: { width: "sm" },
			cell: ({ getValue }) => <ProjectStatusBadge status={getValue()} />,
		}),

		columnHelper.accessor("engagementType", {
			header: "Engagement",
			meta: { width: "sm" },
			cell: ({ getValue }) => engagementTypes[getValue()],
		}),

		columnHelper.accessor("workCountry", {
			header: "Country",
			meta: { align: "center", width: "xxs" },
		}),

		columnHelper.accessor("startsOn", {
			header: "Period",
			meta: { width: "sm" },
			cell: ({ row, getValue }) => (
				<span className="table-number">{formatPeriod(getValue(), row.original.endsOn)}</span>
			),
		}),

		columnHelper.accessor("responsibleContact", {
			header: "Responsible",
			cell: ({ getValue }) => getValue()?.fullname ?? "—",
		}),

		columnHelper.accessor("complianceOutstandingCount", {
			header: "Compliance",
			meta: { align: "center", width: "sm" },
			cell: ({ row, getValue }) => (
				<ComplianceChip
					outstanding={getValue()}
					nextExpiryOn={row.original.nextComplianceExpiryOn}
				/>
			),
		}),
	]);
}
