import { BriefcaseBusiness } from "lucide-react";
import type React from "react";
import { useRef } from "react";
import { Route } from "#/routes/app/positions";
import type { PositionListItem } from "@/api/models";
import { Page } from "@/components/layout";
import { EmptyState, LoadMore } from "@/components/ui";
import {
	type PositionWizardCommand,
	PositionWizardDialog,
} from "../wizards/position/PositionWizardDialog";
import { PositionsToolbar } from "./components/PositionsToolbar";
import { PositionsCardList, PositionsTable } from "./components/table";
import { useArchivePosition, useGetPositionsSlice, useRestorePosition } from "./hooks";

/**
 * The register of roles across every project. The project's own tab answers "what roles does this
 * delivery have"; this page answers "where do we have bricklayers", which is the question that has
 * no home on a project page and is why the roles have a projection of their own.
 */
const PositionsPage: React.FC = () => {
	const wizardRef = useRef<PositionWizardCommand>(null);

	const navigate = Route.useNavigate();
	const search = Route.useSearch();

	const query = useGetPositionsSlice({
		search: search.search,
		projectId: search.projectId,
		contractType: search.contractType,
		includeArchived: search.includeArchived,
	});

	const refresh = () => void query.refetch();

	const archive = useArchivePosition({ onSuccess: refresh });
	const restore = useRestorePosition({ onSuccess: refresh });

	const items = query.data?.pages.flatMap((page) => page.content ?? []) ?? [];
	const hasMore = query.data?.pages.flatMap((page) => page.hasMore ?? [false]) ?? [false];
	const isEmpty = query.isFetched && items.length === 0;

	/* Owned here rather than by each of the two lists, so one role cannot be open twice. */
	const onEdit = (position: PositionListItem) => wizardRef.current?.edit(position.id);

	const onArchive = (position: PositionListItem) =>
		archive.mutation.mutate({ projectId: position.projectId, positionId: position.id });

	const onRestore = (position: PositionListItem) =>
		restore.mutation.mutate({ projectId: position.projectId, positionId: position.id });

	return (
		<>
			<Page
				className="has-mobile-view"
				title="Positions"
				description="The roles open inside projects: what somebody does there, on what contract and for how much."
				onRefresh={refresh}
				loading={query.isPending}
				isEmpty={isEmpty}
				emptyState={
					<EmptyState
						title="No positions found"
						description="A role is opened inside a project. Until one exists, nobody can be planned onto that delivery."
					>
						<BriefcaseBusiness size={24} />
					</EmptyState>
				}
			>
				<PositionsToolbar
					search={search.search ?? ""}
					contractType={search.contractType ?? null}
					includeArchived={search.includeArchived ?? false}
					onSearchChange={(value) => {
						navigate({ search: (previous) => ({ ...previous, search: value }) });
					}}
					onContractTypeChange={(value) => {
						navigate({ search: (previous) => ({ ...previous, contractType: value ?? undefined }) });
					}}
					onIncludeArchivedChange={(value) => {
						navigate({
							search: (previous) => ({ ...previous, includeArchived: value || undefined }),
						});
					}}
					onClear={() => {
						navigate({
							search: () => ({
								search: undefined,
								projectId: undefined,
								contractType: undefined,
								includeArchived: undefined,
							}),
						});
					}}
					onAdd={() => {
						wizardRef.current?.open(search.projectId);
					}}
				/>

				<PositionsTable
					positions={items}
					onEdit={onEdit}
					onArchive={onArchive}
					onRestore={onRestore}
				/>

				<PositionsCardList
					positions={items}
					onEdit={onEdit}
					onArchive={onArchive}
					onRestore={onRestore}
				/>

				<LoadMore
					loading={query.isPending}
					hasNext={hasMore[0]}
					onClick={() => {
						query.fetchNextPage();
					}}
				/>
			</Page>

			<PositionWizardDialog ref={wizardRef} onSuccess={refresh} />
		</>
	);
};

export default PositionsPage;
