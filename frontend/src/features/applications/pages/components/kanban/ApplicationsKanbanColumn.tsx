import type { JobApplicationProjection, JobApplicationStatus } from "#/api/models";
import { Kanban } from "@/components/kanban";
import { LoadMore } from "@/components/ui";
import type { ApplicationFilters } from "../../../searchParams";
import { applicationStatuses } from "../../../types";
import { useGetApplicationsSlice } from "../../hooks";
import { applicationStatusIcons } from "./statusIcons";

const COLUMN_PAGE_SIZE = 10;

interface ApplicationsKanbanColumnProps {
	status: JobApplicationStatus;
	filters: Omit<ApplicationFilters, "status">;
	renderCard: (application: JobApplicationProjection) => React.ReactNode;
}

// no count in the header: the list answers `hasMore`, not a total
export function ApplicationsKanbanColumn({
	status,
	filters,
	renderCard,
}: ApplicationsKanbanColumnProps) {
	const query = useGetApplicationsSlice({ ...filters, status }, { pageSize: COLUMN_PAGE_SIZE });

	const items = query.data?.pages.flatMap((page) => page.content ?? []) ?? [];
	const hasMore = query.data?.pages.at(-1)?.hasMore ?? false;

	const label = applicationStatuses[status];
	const Icon = applicationStatusIcons[status];

	return (
		<Kanban.Column id={status} label={label}>
			<Kanban.ColumnHeader title={label} icon={<Icon size={15} />} />

			<Kanban.ColumnContent>
				{query.isLoading ? <Kanban.Empty>Loading ...</Kanban.Empty> : null}

				{query.isError ? <Kanban.Empty>Unable to load applications.</Kanban.Empty> : null}

				{!query.isLoading && !query.isError && items.length === 0 ? (
					<Kanban.Empty>No applications</Kanban.Empty>
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
