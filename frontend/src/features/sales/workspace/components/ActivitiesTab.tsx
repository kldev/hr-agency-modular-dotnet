import { createColumnHelper, useTable } from "@tanstack/react-table";
import { MessageSquare } from "lucide-react";
import type { ActivityProjection } from "#/api/models";
import MainTable from "#/components/table/MainTable";
import { appTableFeatures, type appTableFeaturesType } from "#/components/table/tableFeatures";
import { Button, LoadMore } from "#/components/ui";
import { activityIcons } from "#/features/sales/activityIcons";
import { useGetCompanyActivities } from "#/features/sales/hooks";
import { activityTypeLabels } from "#/features/sales/types";
import { formatDateTime } from "#/utlis";
import { TabState } from "./TabState";

const columnHelper = createColumnHelper<appTableFeaturesType, ActivityProjection>();

const columns = columnHelper.columns([
	columnHelper.accessor("createdAt", {
		header: "Date",
		meta: { width: "md" },
		cell: ({ getValue }) => <span className="table-figure">{formatDateTime(getValue())}</span>,
	}),
	columnHelper.accessor("activityType", {
		header: "Type",
		meta: { width: "sm" },
		cell: ({ getValue }) => {
			const Icon = activityIcons[getValue()];

			return (
				<span className="workspace-activity-type">
					<Icon size={14} />
					{activityTypeLabels[getValue()]}
				</span>
			);
		},
	}),
	columnHelper.accessor("createdBy.fullname", {
		header: "User",
		meta: { width: "md" },
		cell: ({ getValue }) => <span className="truncate">{getValue()}</span>,
	}),
	columnHelper.accessor("opportunityTitle", {
		header: "Opportunity",
		meta: { width: "xl" },
		cell: ({ getValue }) => <span className="truncate">{getValue() || "—"}</span>,
	}),
	columnHelper.accessor("note", {
		header: "Description",
		cell: ({ getValue }) => (
			<span className="workspace-note" title={getValue()}>
				{getValue() || "—"}
			</span>
		),
	}),
]);

interface ActivitiesTabProps {
	companyId: string;
	onLog: () => void;
}

/** What happened on every deal with the company, newest first - filtered by the API, not here. */
export function ActivitiesTab({ companyId, onLog }: ActivitiesTabProps) {
	const query = useGetCompanyActivities(companyId);
	const items = query.data?.pages.flatMap((page) => page.content) ?? [];

	const table = useTable(
		{
			features: appTableFeatures,
			columns,
			data: items,
			getRowId: (activity) => activity.id,
			enableSorting: false,
		},
		(state) => ({ sorting: state.sorting }),
	);

	return (
		<section className="data-details-section" aria-label="Activities">
			<div className="data-details-section-header">
				<div>
					<h2>Activities</h2>
					<p>Latest activity across all the company's opportunities</p>
				</div>

				<Button icon={<MessageSquare size={14} />} onClick={onLog}>
					Log activity
				</Button>
			</div>

			<TabState
				isLoading={query.isLoading}
				isError={query.isError}
				isEmpty={items.length === 0}
				what="Activities"
				emptyIcon={<MessageSquare size={22} />}
				emptyDescription="Calls, e-mails and meetings logged on this company's deals show up here."
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
