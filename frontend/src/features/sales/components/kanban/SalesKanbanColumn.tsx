import type { OpportunityProjection, OpportunityStage } from "#/api/models";
import { formatSalary } from "#/utlis";
import { Kanban } from "@/components/kanban";
import { LoadMore } from "@/components/ui";
import { type SalesPageFillters, type StageTotals, useGetOpportunitesSlice } from "../../hooks";
import { salesStageOptions } from "../../types";
import { stageIcons } from "./stages";

const COLUMN_PAGE_SIZE = 10;

interface SalesKanbanColumnProps {
	stage: OpportunityStage;
	filters: SalesPageFillters;
	totals: StageTotals;
	renderCard: (item: OpportunityProjection) => React.ReactNode;
}

function formatValues(totals: StageTotals) {
	if (totals.values.length === 0) {
		return "0";
	}

	return totals.values.map((it) => `${formatSalary(it.value)} ${it.currency}`).join(" · ");
}

export function SalesKanbanColumn({ stage, filters, totals, renderCard }: SalesKanbanColumnProps) {
	const query = useGetOpportunitesSlice({ ...filters, stage }, { pageSize: COLUMN_PAGE_SIZE });

	const items = query.data?.pages.flatMap((page) => page.content ?? []) ?? [];
	const hasMore = query.data?.pages.at(-1)?.hasMore ?? false;

	const label = salesStageOptions[stage];
	const Icon = stageIcons[stage];

	return (
		<Kanban.Column label={`${label}, ${totals.count} opportunities`}>
			<Kanban.ColumnHeader
				title={label}
				icon={<Icon size={15} />}
				count={totals.count}
				meta={formatValues(totals)}
			/>

			<Kanban.ColumnContent>
				{query.isLoading ? <Kanban.Empty>Loading ...</Kanban.Empty> : null}

				{query.isError ? <Kanban.Empty>Unable to load opportunities.</Kanban.Empty> : null}

				{!query.isLoading && !query.isError && items.length === 0 ? (
					<Kanban.Empty>No opportunities</Kanban.Empty>
				) : null}

				{items.map((item) => renderCard(item))}
			</Kanban.ColumnContent>

			<Kanban.ColumnFooter>
				<LoadMore
					hasNext={hasMore}
					loading={query.isFetchingNextPage}
					onClick={() => query.fetchNextPage()}
				/>
			</Kanban.ColumnFooter>
		</Kanban.Column>
	);
}
