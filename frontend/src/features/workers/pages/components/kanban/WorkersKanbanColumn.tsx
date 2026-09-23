import type { WorkerProjection, WorkerStatus } from "#/api/models";
import { Kanban } from "@/components/kanban";
import { LoadMore } from "@/components/ui";
import { workerStatuses } from "../../../types";
import { useGetWorkersSlice, type WorkersFilters } from "../../hooks";
import { workerStatusIcons } from "./statusIcons";

const COLUMN_PAGE_SIZE = 10;

interface WorkersKanbanColumnProps {
	status: WorkerStatus;
	filters: WorkersFilters;
	renderCard: (worker: WorkerProjection) => React.ReactNode;
}

// no count in the header: the list answers `hasMore`, not a total
export function WorkersKanbanColumn({ status, filters, renderCard }: WorkersKanbanColumnProps) {
	const query = useGetWorkersSlice(
		{ ...filters, status: [status] },
		{ pageSize: COLUMN_PAGE_SIZE },
	);

	const items = query.data?.pages.flatMap((page) => page.content ?? []) ?? [];
	const hasMore = query.data?.pages.at(-1)?.hasMore ?? false;

	const label = workerStatuses[status];
	const Icon = workerStatusIcons[status];

	return (
		<Kanban.Column id={status} label={label}>
			<Kanban.ColumnHeader title={label} icon={<Icon size={15} />} />

			<Kanban.ColumnContent>
				{query.isLoading ? <Kanban.Empty>Loading ...</Kanban.Empty> : null}

				{query.isError ? <Kanban.Empty>Unable to load people.</Kanban.Empty> : null}

				{!query.isLoading && !query.isError && items.length === 0 ? (
					<Kanban.Empty>Nobody here</Kanban.Empty>
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
