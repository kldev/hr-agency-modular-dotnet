import { Building2 } from "lucide-react";
import { useRef } from "react";
import { Route } from "#/routes/app/legal-entities";
import { Page } from "@/components/layout";
import { EmptyState, LoadMore } from "@/components/ui";
import {
	type CloseLegalEntityCommand,
	CloseLegalEntityDrawer,
	type LegalEntityFormCommand,
	LegalEntityFormDrawer,
} from "../drawers";
import { LegalEntitiesTable, LegalEntitiesToolbar } from "./components";
import { type LegalEntitiesFilters, useGetLegalEntitiesSlice } from "./hooks";
import "./legal-entities.css";

export function LegalEntitiesPage() {
	const formRef = useRef<LegalEntityFormCommand>(null);
	const closeRef = useRef<CloseLegalEntityCommand>(null);

	const navigate = Route.useNavigate();
	const search: LegalEntitiesFilters = Route.useSearch();

	const query = useGetLegalEntitiesSlice(search);

	const items = query.data?.pages.flatMap((page) => page.content ?? []) ?? [];
	const hasMore = query.data?.pages.at(-1)?.hasMore ?? false;
	const isEmpty = query.isFetched && items.length === 0;

	const refresh = () => {
		void query.refetch();
	};

	return (
		<>
			<Page
				className="has-mobile-view"
				title="Legal entities"
				description="The companies our agency trades through"
				onRefresh={refresh}
				loading={query.isPending}
				isEmpty={isEmpty}
				emptyState={
					<EmptyState
						title="No legal entities yet"
						description="A project is delivered by one of these, so at least one is needed before a project can be set up."
					>
						<Building2 size={24} />
					</EmptyState>
				}
			>
				<LegalEntitiesToolbar
					search={search.search ?? ""}
					activeOnly={search.activeOnly ?? false}
					onSearchChange={(value) => {
						navigate({ search: (previous) => ({ ...previous, search: value }) });
					}}
					onActiveOnlyChange={(value) => {
						navigate({
							search: (previous) => ({ ...previous, activeOnly: value || undefined }),
						});
					}}
					onAdd={() => formRef.current?.create()}
				/>

				<LegalEntitiesTable
					entities={items}
					onEdit={(entity) => formRef.current?.edit(entity)}
					onClose={(entity) => closeRef.current?.close(entity)}
				/>

				<LoadMore
					hasNext={hasMore}
					loading={query.isFetchingNextPage}
					onClick={() => void query.fetchNextPage()}
				/>
			</Page>

			<LegalEntityFormDrawer ref={formRef} onSuccess={refresh} />
			<CloseLegalEntityDrawer ref={closeRef} onSuccess={refresh} />
		</>
	);
}
