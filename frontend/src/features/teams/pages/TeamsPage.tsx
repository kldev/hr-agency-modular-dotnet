import { Users } from "lucide-react";
import type React from "react";
import { useRef } from "react";

import { Route } from "#/routes/app/teams";
import { Page } from "@/components/layout";
import { EmptyState, LoadMore } from "@/components/ui";
import {
	CreateTeamDrawer,
	type CreateTeamFormCommand,
	TeamsCardList,
	TeamsTable,
	TeamsToolbar,
} from "../components";
import { type TeamsFilters, useGetTeamsSlice } from "./hooks";

const TeamsPage: React.FC = () => {
	const formRef = useRef<CreateTeamFormCommand>(null);

	const navigate = Route.useNavigate();
	const search: TeamsFilters = Route.useSearch();

	const query = useGetTeamsSlice(search);
	const items = query.data?.pages.flatMap((page) => page.content ?? []) ?? [];
	const hasMore = query.data?.pages.at(-1)?.hasMore ?? false;
	const isEmpty = query.isFetched && items.length === 0;

	return (
		<Page
			className="has-mobile-view"
			title="Teams"
			description="Groups of people working an account together"
			onRefresh={() => query.refetch()}
			loading={query.isPending}
			isEmpty={isEmpty}
			emptyState={
				<EmptyState title="No teams found">
					<Users size={24} />
				</EmptyState>
			}
		>
			<TeamsToolbar
				search={search.search ?? ""}
				onClear={() => {
					navigate({ search: { search: undefined } });
				}}
				onSearchChange={(v) => {
					navigate({ search: (previous) => ({ ...previous, search: v }) });
				}}
				onAdd={() => {
					formRef.current?.create();
				}}
			/>

			<TeamsTable teams={items} onRefresh={() => query.refetch()} />
			<TeamsCardList items={items} />

			<LoadMore
				loading={query.isPending}
				hasNext={hasMore}
				onClick={() => {
					query.fetchNextPage();
				}}
			/>

			<CreateTeamDrawer
				ref={formRef}
				onSuccess={() => {
					query.refetch();
				}}
			/>
		</Page>
	);
};

export default TeamsPage;
