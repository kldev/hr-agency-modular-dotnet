import { Link } from "@tanstack/react-router";
import { createColumnHelper, useTable } from "@tanstack/react-table";
import { Target } from "lucide-react";
import type { OpportunityProjection } from "#/api/models";
import MainTable from "#/components/table/MainTable";
import { appTableFeatures, type appTableFeaturesType } from "#/components/table/tableFeatures";
import { ItemMark, LoadMore } from "#/components/ui";
import { SalesStageBadge } from "#/features/sales/components";
import { useGetOpportunitesSlice } from "#/features/sales/hooks";
import { activityTypeLabels } from "#/features/sales/types";
import { relativeDayLabel } from "#/features/tasks/relativeDay";
import { formatDate, formatSalary } from "#/utlis";
import { TabState } from "./TabState";

const columnHelper = createColumnHelper<appTableFeaturesType, OpportunityProjection>();

const columns = columnHelper.columns([
	columnHelper.accessor("title", {
		header: "Name",
		meta: { width: "2xl" },
		cell: ({ row, getValue }) => (
			<div className="table-cell-content">
				<ItemMark name={getValue()} />
				<Link
					to="/app/sales/opportunities/$id"
					params={{ id: row.original.id }}
					className="data-name truncate"
				>
					{getValue()}
				</Link>
			</div>
		),
	}),
	columnHelper.accessor("stage", {
		header: "Stage",
		meta: { width: "sm" },
		cell: ({ getValue }) => <SalesStageBadge stage={getValue()} />,
	}),
	columnHelper.accessor("expectedValue", {
		header: "Value",
		meta: { width: "sm" },
		cell: ({ row, getValue }) => (
			<span className="table-figure">
				{formatSalary(Number(getValue()))} {row.original.currencyCode}
			</span>
		),
	}),
	columnHelper.accessor("responsible.fullname", {
		header: "Owner",
		meta: { width: "md" },
		cell: ({ getValue }) => <span className="truncate">{getValue()}</span>,
	}),
	columnHelper.accessor("expectedCloseDate", {
		header: "Expected close",
		meta: { width: "sm" },
		cell: ({ getValue }) => <span className="table-figure">{formatDate(getValue())}</span>,
	}),
	columnHelper.accessor("lastActivityAt", {
		header: "Last activity",
		cell: ({ row, getValue }) => {
			const at = getValue();

			return at ? (
				<span className="table-figure">
					{relativeDayLabel(at)}
					{row.original.lastActivityType
						? ` · ${activityTypeLabels[row.original.lastActivityType]}`
						: ""}
				</span>
			) : (
				<span className="data-meta">No activity yet</span>
			);
		},
	}),
]);

/** The company's deals, all stages, from the same list the sales page reads. */
export function OpportunitiesTab({ companyId }: { companyId: string }) {
	const query = useGetOpportunitesSlice({ companyId }, { pageSize: 20 });
	const items = query.data?.pages.flatMap((page) => page.content) ?? [];

	const table = useTable(
		{
			features: appTableFeatures,
			columns,
			data: items,
			getRowId: (opportunity) => opportunity.id,
			enableSorting: false,
		},
		(state) => ({ sorting: state.sorting }),
	);

	return (
		<section className="data-details-section" aria-label="Opportunities">
			<div className="data-details-section-header">
				<div>
					<h2>Opportunities</h2>
					<p>Every deal with the company, from first contact to won or lost</p>
				</div>
			</div>

			<TabState
				isLoading={query.isLoading}
				isError={query.isError}
				isEmpty={items.length === 0}
				what="Opportunities"
				emptyIcon={<Target size={22} />}
				emptyDescription="Deals recorded for this company on the sales page show up here."
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
