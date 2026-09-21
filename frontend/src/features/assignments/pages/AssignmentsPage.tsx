import { Briefcase } from "lucide-react";
import type React from "react";
import { useRef } from "react";
import type { AssignmentProjection } from "@/api/models";
import { Page } from "@/components/layout";
import { EmptyState, LoadMore } from "@/components/ui";
import {
	ChangeAssignmentStatusDrawer,
	type ChangeAssignmentStatusFormCommand,
	EditAssignmentDrawer,
	type EditAssignmentFormCommand,
} from "../drawers";
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
	const editRef = useRef<EditAssignmentFormCommand>(null);
	const statusRef = useRef<ChangeAssignmentStatusFormCommand>(null);

	const query = useGetAssignmentsSlice({
		search: search.search,
		status: search.status ? [search.status] : undefined,
		workerId: search.workerId,
		projectId: search.projectId,
	});

	const items = query.data?.pages.flatMap((page) => page.content ?? []) ?? [];
	const hasMore = query.data?.pages.flatMap((page) => page.hasMore ?? [false]) ?? [false];
	const isEmpty = query.isFetched && items.length === 0;

	const refresh = () => void query.refetch();

	/* Owned here rather than by the table and the card list, so one record cannot be open twice. */
	const onEdit = (assignment: AssignmentProjection) => editRef.current?.edit(assignment);

	const onChangeStatus = (assignment: AssignmentProjection) =>
		statusRef.current?.changeStatus(assignment);

	return (
		<>
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

				<AssignmentsTable assignments={items} onEdit={onEdit} onChangeStatus={onChangeStatus} />

				<AssignmentsCardList assignments={items} onEdit={onEdit} onChangeStatus={onChangeStatus} />

				<LoadMore
					loading={query.isPending}
					hasNext={hasMore[0]}
					onClick={() => {
						query.fetchNextPage();
					}}
				/>
			</Page>

			<EditAssignmentDrawer ref={editRef} onSuccess={refresh} />
			<ChangeAssignmentStatusDrawer ref={statusRef} onSuccess={refresh} />
		</>
	);
};

export default AssignmentsPage;
