import { useQueryClient } from "@tanstack/react-query";
import { DollarSign } from "lucide-react";
import type React from "react";
import { useRef, useState } from "react";
import { useGetOnlyMine } from "#/hooks";
import { Route } from "#/routes/app/sales";

import { salesKeys } from "@/api/query-keys";
import { Page } from "@/components/layout";
import { EmptyState, EnumFilter, LoadMore } from "@/components/ui";
import {
	CreateOpportunityDrawer,
	type CreateOpportunityRef,
	SalesCardList,
	SalesKanban,
	SalesTable,
} from "../components";
import { SalesToolbar } from "../components/SalesToolbar";
import { useGetOpportunitesSlice } from "../hooks";
import { salesStageOptions } from "../types";
import "./sales-kanban.css";

const SalesPage: React.FC = () => {
	const oppRef = useRef<CreateOpportunityRef>(null);
	const [onlyMine, setOnlyMine] = useState<boolean>(false);

	const client = useQueryClient();
	const navigate = Route.useNavigate();
	const search = Route.useSearch();

	const view = search.view ?? "table";
	const isTable = view === "table";

	const { userId } = useGetOnlyMine(onlyMine);

	const filters = {
		search: search.search,
		responsibleId: userId,
	};

	const query = useGetOpportunitesSlice({ ...filters, stage: search.stage }, { enabled: isTable });
	const items = query.data?.pages.flatMap((page) => page.content ?? []) ?? [];
	const hasMore = query.data?.pages.flatMap((page) => page.hasMore ?? [false]) ?? [false];
	const isEmpty = isTable && query.isFetched && items.length === 0;
	const onRefresh = () => {
		if (isTable) {
			query.refetch();
			return;
		}

		client.invalidateQueries({ queryKey: salesKeys.all });
	};

	return (
		<Page
			className="has-mobile-view"
			wide={!isTable}
			title="Sales"
			description="Manage your leads and sales opportunities THROUGH the pipeline."
			onRefresh={onRefresh}
			loading={isTable && query.isPending}
			isEmpty={isEmpty}
			emptyState={
				<EmptyState title="No sales opportunities found">
					<DollarSign size={24} />
				</EmptyState>
			}
		>
			<SalesToolbar
				search={search.search ?? ""}
				onClear={() => {
					navigate({
						search: (previous) => ({ search: undefined, stage: undefined, view: previous.view }),
					});
				}}
				onSearchChange={(v) => {
					navigate({ search: (previous) => ({ ...previous, search: v }) });
				}}
				onAdd={() => {
					oppRef?.current?.create();
				}}
				onlyMine={onlyMine}
				onlyMineOnChange={(val) => {
					setOnlyMine(val);
					onRefresh();
				}}
				view={view}
				onViewChange={(val) => {
					navigate({
						search: (previous) => ({ ...previous, view: val === "table" ? undefined : val }),
					});
				}}
			/>

			{isTable ? (
				<>
					<EnumFilter
						value={search.stage || null}
						options={salesStageOptions}
						onChange={(s) => {
							navigate({ search: (previous) => ({ ...previous, stage: s ?? undefined }) });
						}}
					/>
					<SalesTable items={items} onRefresh={onRefresh} />
					<SalesCardList items={items} onRefresh={onRefresh} />
					<LoadMore
						loading={query.isPending}
						hasNext={hasMore[0]}
						onClick={() => {
							query.fetchNextPage();
						}}
					/>
				</>
			) : (
				<SalesKanban filters={filters} />
			)}
			<CreateOpportunityDrawer ref={oppRef} onSuccess={() => query.refetch()} />
		</Page>
	);
};

export default SalesPage;
