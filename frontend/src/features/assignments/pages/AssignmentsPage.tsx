import { Briefcase } from "lucide-react";
import type React from "react";
import { Page } from "@/components/layout";
import { EmptyState, LoadMore } from "@/components/ui";
import type { AssignmentsSearch } from "../searchParams";
import { AssignmentsToolbar } from "./components/AssignmentsToolbar";
import { AssignmentsCardList, AssignmentsTable } from "./components/table";
import { useGetAssignmentsSlice } from "./hooks";

interface AssignmentsPageProps {
	search: AssignmentsSearch;
	onSearchChange: (next: Partial<AssignmentsSearch>) => void;
	onClear: () => void;
}

/** Who is posted where, across every project - the view neither the worker nor the project gives. */
const AssignmentsPage: React.FC<AssignmentsPageProps> = ({ search, onSearchChange, onClear }) => {
	const query = useGetAssignmentsSlice({
		search: search.search,
		status: search.status ? [search.status] : undefined,
		workerId: search.workerId,
		projectId: search.projectId,
	});

	const items = query.data?.pages.flatMap((page) => page.content ?? []) ?? [];
	const hasMore = query.data?.pages.flatMap((page) => page.hasMore ?? [false]) ?? [false];
	const isEmpty = query.isFetched && items.length === 0;

	return (
		<Page
			className="has-mobile-view"
			title="Assignments"
			description="One person on one project for one period, with the papers that go with it."
			onRefresh={() => query.refetch()}
			loading={query.isPending}
			isEmpty={isEmpty}
			emptyState={
				<EmptyState title="No assignments found">
					<Briefcase size={24} />
				</EmptyState>
			}
		>
			<AssignmentsToolbar
				search={search.search ?? ""}
				status={search.status ?? null}
				onSearchChange={(value) => onSearchChange({ search: value })}
				onStatusChange={(value) => onSearchChange({ status: value ?? undefined })}
				onClear={onClear}
			/>

			<AssignmentsTable assignments={items} />

			<AssignmentsCardList assignments={items} />

			<LoadMore
				loading={query.isPending}
				hasNext={hasMore[0]}
				onClick={() => {
					query.fetchNextPage();
				}}
			/>
		</Page>
	);
};

export default AssignmentsPage;
