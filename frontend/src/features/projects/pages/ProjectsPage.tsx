import { FolderKanban } from "lucide-react";
import type React from "react";
import { useRef } from "react";
import { Route } from "#/routes/app/projects";
import type { ProjectProjection } from "@/api/models";
import { Page } from "@/components/layout";
import { EmptyState, LoadMore } from "@/components/ui";
import { ChangeProjectStatusDrawer, type ChangeProjectStatusFormCommand } from "../drawers";
import {
	type ProjectWizardCommand,
	ProjectWizardDialog,
} from "../wizards/project/ProjectWizardDialog";
import { ProjectsToolbar } from "./components/ProjectsToolbar";
import { ProjectsCardList, ProjectsTable } from "./components/table";
import { useGetProjectsSlice } from "./hooks";

const ProjectsPage: React.FC = () => {
	const wizardRef = useRef<ProjectWizardCommand>(null);
	const statusRef = useRef<ChangeProjectStatusFormCommand>(null);

	const navigate = Route.useNavigate();
	const search = Route.useSearch();

	const query = useGetProjectsSlice({
		search: search.search,
		status: search.status ? [search.status] : undefined,
		companyId: search.companyId,
		country: search.country,
	});

	const items = query.data?.pages.flatMap((page) => page.content ?? []) ?? [];
	const hasMore = query.data?.pages.flatMap((page) => page.hasMore ?? [false]) ?? [false];
	const isEmpty = query.isFetched && items.length === 0;

	const refresh = () => void query.refetch();

	/* Owned here rather than by each of the two lists, so one project cannot be open twice. */
	const onEdit = (project: ProjectProjection) => wizardRef.current?.edit(project);

	const onChangeStatus = (project: ProjectProjection) => statusRef.current?.changeStatus(project);

	return (
		<>
			<Page
				className="has-mobile-view"
				title="Projects"
				description="Client engagements in delivery: contracts, contacts, documents and compliance."
				onRefresh={() => query.refetch()}
				loading={query.isPending}
				isEmpty={isEmpty}
				emptyState={
					<EmptyState title="No projects found">
						<FolderKanban size={24} />
					</EmptyState>
				}
			>
				<ProjectsToolbar
					search={search.search ?? ""}
					status={search.status ?? null}
					onSearchChange={(value) => {
						navigate({ search: (previous) => ({ ...previous, search: value }) });
					}}
					onStatusChange={(value) => {
						navigate({ search: (previous) => ({ ...previous, status: value ?? undefined }) });
					}}
					onClear={() => {
						navigate({
							search: () => ({
								search: undefined,
								status: undefined,
								companyId: undefined,
								country: undefined,
							}),
						});
					}}
					onAdd={() => {
						wizardRef.current?.create();
					}}
				/>

				<ProjectsTable projects={items} onEdit={onEdit} onChangeStatus={onChangeStatus} />

				<ProjectsCardList projects={items} onEdit={onEdit} onChangeStatus={onChangeStatus} />

				<LoadMore
					loading={query.isPending}
					hasNext={hasMore[0]}
					onClick={() => {
						query.fetchNextPage();
					}}
				/>
			</Page>

			<ChangeProjectStatusDrawer ref={statusRef} onSuccess={refresh} />

			<ProjectWizardDialog
				ref={wizardRef}
				onSuccess={(projectId: string) => {
					navigate({
						to: "/app/projects/$id",
						params: { id: projectId },
						search: { search: undefined, tab: undefined },
					});
				}}
			/>
		</>
	);
};

export default ProjectsPage;
