import { Link } from "@tanstack/react-router";
import { createColumnHelper, useTable } from "@tanstack/react-table";
import { FolderKanban } from "lucide-react";
import type { ProjectProjection } from "#/api/models";
import MainTable from "#/components/table/MainTable";
import { appTableFeatures, type appTableFeaturesType } from "#/components/table/tableFeatures";
import { ItemMark, LoadMore, ProjectStatusBadge } from "#/components/ui";
import { useGetProjectsSlice } from "#/features/projects/pages/hooks";
import { formatDate } from "#/utlis";
import { TabState } from "./TabState";

const columnHelper = createColumnHelper<appTableFeaturesType, ProjectProjection>();

const columns = columnHelper.columns([
	columnHelper.accessor("name", {
		header: "Name",
		meta: { width: "2xl" },
		cell: ({ row, getValue }) => (
			<div className="table-cell-content">
				<ItemMark name={getValue()} />
				<Link
					to="/app/projects/$id"
					params={{ id: row.original.id }}
					search={{ search: undefined, tab: undefined }}
					className="data-name truncate"
				>
					{getValue()}
				</Link>
			</div>
		),
	}),
	columnHelper.accessor("status", {
		header: "Status",
		meta: { width: "sm" },
		cell: ({ getValue }) => <ProjectStatusBadge status={getValue()} />,
	}),
	columnHelper.accessor("peopleCount", {
		header: "People",
		meta: { width: "ssm", align: "center" },
		cell: ({ getValue }) => <span className="table-figure">{Number(getValue() ?? 0)}</span>,
	}),
	columnHelper.accessor("startsOn", {
		header: "Start",
		meta: { width: "sm" },
		cell: ({ getValue }) => <span className="table-figure">{formatDate(getValue())}</span>,
	}),
	columnHelper.accessor("teamName", {
		header: "Team",
		meta: { width: "md" },
		cell: ({ getValue }) => <span className="truncate">{getValue() ?? "—"}</span>,
	}),
	columnHelper.accessor("opportunity", {
		header: "Opportunity",
		cell: ({ getValue }) => {
			const opportunity = getValue();

			return opportunity ? (
				<Link
					to="/app/sales/opportunities/$id"
					params={{ id: opportunity.id }}
					className="truncate"
				>
					{opportunity.title}
				</Link>
			) : (
				<span className="data-meta">—</span>
			);
		},
	}),
]);

/** Deliveries for the company, with the people on them and the deal each was sold as. */
export function ProjectsTab({ companyId }: { companyId: string }) {
	const query = useGetProjectsSlice({ companyId });
	const items = query.data?.pages.flatMap((page) => page.content) ?? [];

	const table = useTable(
		{
			features: appTableFeatures,
			columns,
			data: items,
			getRowId: (project) => project.id,
			enableSorting: false,
		},
		(state) => ({ sorting: state.sorting }),
	);

	return (
		<section className="data-details-section" aria-label="Projects">
			<div className="data-details-section-header">
				<div>
					<h2>Projects</h2>
					<p>What the agency delivers for the company</p>
				</div>
			</div>

			<TabState
				isLoading={query.isLoading}
				isError={query.isError}
				isEmpty={items.length === 0}
				what="Projects"
				emptyIcon={<FolderKanban size={22} />}
				emptyDescription="Projects set up for this company on the projects page show up here."
			>
				<MainTable table={table} />
				<LoadMore
					hasNext={Boolean(query.hasNextPage)}
					loading={query.isFetchingNextPage}
					onClick={() => query.fetchNextPage()}
				/>
			</TabState>
		</section>
	);
}
