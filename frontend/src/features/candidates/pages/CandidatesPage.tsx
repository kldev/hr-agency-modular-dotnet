import { Users } from "lucide-react";
import type React from "react";
import { useRef } from "react";
import { Route } from "#/routes/app/candidates";

import { Page } from "@/components/layout";

import { EmptyState, LoadMore } from "@/components/ui";
import {
	CandidatesCardList,
	CandidatesTable,
	CandidatesToolbar,
	CreateCandidateDrawer,
	type CreateCandidateFormCommand,
} from "../components";
import { type CandidatesPageFillter, useGetCandidatesSlice } from "../hooks";

const CandidatesPage: React.FC = () => {
	const formRef = useRef<CreateCandidateFormCommand>(null);
	const navigate = Route.useNavigate();

	const search = Route.useSearch() as CandidatesPageFillter;
	const query = useGetCandidatesSlice(search);
	const items = query.data?.pages.flatMap((page) => page.content ?? []) ?? [];
	const hasMore = query.data?.pages.flatMap((page) => page.hasMore ?? [false]) ?? [false];
	const isEmpty = query.isFetched && items.length === 0;
	const onRefresh = () => {
		query.refetch();
	};

	return (
		<Page
			className="has-mobile-view"
			title="Candidates"
			description="Manage candidates and their recruitment profiles."
			onRefresh={onRefresh}
			loading={query.isPending}
			isEmpty={isEmpty}
			emptyState={
				<EmptyState title="No candidates found">
					<Users size={24} />
				</EmptyState>
			}
		>
			<CandidatesToolbar
				search={search.search ?? ""}
				onClear={() => {
					navigate({ search: {} });
				}}
				onSearchChange={(v) => {
					navigate({ search: (previous) => ({ ...previous, search: v }) });
				}}
				source={search.source || null}
				onSourceChange={(s) => {
					navigate({ search: (previous) => ({ ...previous, source: s }) });
				}}
				onAdd={() => {
					formRef.current?.create();
				}}
			/>
			<CandidatesTable items={items} onRefresh={onRefresh} />
			<CandidatesCardList items={items} onRefresh={onRefresh} />
			<LoadMore
				loading={query.isPending}
				hasNext={hasMore[0]}
				onClick={() => {
					query.fetchNextPage();
				}}
			/>
			<CreateCandidateDrawer ref={formRef} onSuccess={onRefresh} />
		</Page>
	);
};

export default CandidatesPage;
