import { useQueryClient } from "@tanstack/react-query";
import { HardHat } from "lucide-react";
import type React from "react";
import { useRef } from "react";
import {
	type PlanAssignmentWizardCommand,
	PlanAssignmentWizardDialog,
} from "#/features/assignments/wizards/plan/PlanAssignmentWizardDialog";
import type { WorkerProjection, WorkerStatus } from "@/api/models";
import { workersKeys } from "@/api/query-keys";
import { Page } from "@/components/layout";
import { EmptyState, LoadMore } from "@/components/ui";
import { ChangeWorkerStatusDrawer, type ChangeWorkerStatusFormCommand } from "../drawers";
import type { WorkersSearch } from "../searchParams";
import { HOME_WORK_COUNTRY } from "../types";
import { type WorkerWizardCommand, WorkerWizardDialog } from "../wizards/worker/WorkerWizardDialog";
import { WorkersKanban } from "./components/kanban";
import { WorkersCardList, WorkersTable } from "./components/table";
import { WorkersToolbar } from "./components/WorkersToolbar";
import { useGetWorkersSlice } from "./hooks";

/**
 * `all` is the whole register; `abroad` is the desk that looks after people working outside the
 * home country.
 *
 * The two are not symmetrical and that is deliberate on the API's side: `workCountry=[PL]` keeps
 * only people who are currently on a Polish posting, which drops everybody who has no posting yet,
 * while `excludeWorkCountry=[PL]` keeps them. So the first view filters on nothing at all - a
 * register that hides the person you registered five minutes ago is not a register.
 */
export type WorkersScope = "all" | "abroad";

interface WorkersPageProps {
	scope: WorkersScope;
	search: WorkersSearch;
	/** Merged into the URL by the route that owns it; the two routes have different search types. */
	onSearchChange: (next: Partial<WorkersSearch>) => void;
	onClear: () => void;
}

const WorkersPage: React.FC<WorkersPageProps> = ({ scope, search, onSearchChange, onClear }) => {
	const wizardRef = useRef<WorkerWizardCommand>(null);
	const statusRef = useRef<ChangeWorkerStatusFormCommand>(null);
	const planAssignmentRef = useRef<PlanAssignmentWizardCommand>(null);
	const client = useQueryClient();

	const view = search.view ?? "table";
	const isTable = view === "table";

	const filters = {
		search: search.search,
		citizenship: search.citizenship,
		excludeWorkCountry: scope === "abroad" ? [HOME_WORK_COUNTRY] : undefined,
	};

	const query = useGetWorkersSlice(
		{ ...filters, status: search.status ? [search.status] : undefined },
		{ enabled: isTable },
	);

	const items = query.data?.pages.flatMap((page) => page.content ?? []) ?? [];
	const hasMore = query.data?.pages.flatMap((page) => page.hasMore ?? [false]) ?? [false];
	const isEmpty = isTable && query.isFetched && items.length === 0;

	// on the board a change moves a card between two columns, so every column is refetched
	const refresh = () => {
		if (isTable) {
			void query.refetch();
			return;
		}

		void client.invalidateQueries({ queryKey: workersKeys.lists() });
	};

	const onEdit = (worker: WorkerProjection) => wizardRef.current?.edit(worker);
	const onChangeStatus = (worker: WorkerProjection, target?: WorkerStatus) =>
		statusRef.current?.changeStatus(worker, target);

	const onPlanAssignment = (worker: WorkerProjection) =>
		planAssignmentRef.current?.plan({ workerId: worker.id ?? "" });

	return (
		<>
			<Page
				className="has-mobile-view"
				wide={!isTable}
				title={scope === "abroad" ? "Workers abroad" : "Workers"}
				description={
					scope === "abroad"
						? "People on a posting outside the home country, and everybody not yet placed."
						: "The register of people: who they are, where they work and what still has to be in order."
				}
				onRefresh={refresh}
				loading={isTable && query.isPending}
				isEmpty={isEmpty}
				emptyState={
					<EmptyState title="No people found">
						<HardHat size={24} />
					</EmptyState>
				}
			>
				<WorkersToolbar
					search={search.search ?? ""}
					status={search.status ?? null}
					onSearchChange={(value) => onSearchChange({ search: value })}
					onStatusChange={(value) => onSearchChange({ status: value ?? undefined })}
					onClear={onClear}
					onAdd={() => wizardRef.current?.register()}
					view={view}
					onViewChange={(next) => onSearchChange({ view: next === "table" ? undefined : next })}
				/>

				{isTable ? (
					<>
						<WorkersTable
							workers={items}
							onEdit={onEdit}
							onChangeStatus={onChangeStatus}
							onPlanAssignment={onPlanAssignment}
						/>

						<WorkersCardList
							workers={items}
							onEdit={onEdit}
							onChangeStatus={onChangeStatus}
							onPlanAssignment={onPlanAssignment}
						/>

						<LoadMore
							loading={query.isPending}
							hasNext={hasMore[0]}
							onClick={() => {
								query.fetchNextPage();
							}}
						/>
					</>
				) : (
					<WorkersKanban
						filters={filters}
						onEdit={onEdit}
						onChangeStatus={onChangeStatus}
						onPlanAssignment={onPlanAssignment}
					/>
				)}
			</Page>

			<WorkerWizardDialog ref={wizardRef} onSuccess={refresh} />
			<ChangeWorkerStatusDrawer ref={statusRef} onSuccess={refresh} />
			<PlanAssignmentWizardDialog ref={planAssignmentRef} onSuccess={refresh} />
		</>
	);
};

export default WorkersPage;
