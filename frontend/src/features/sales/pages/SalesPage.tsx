import { DollarSign } from "lucide-react";
import type React from "react";
import { useCallback, useState } from "react";
import { getApiSalesOpportunity } from "@/api/endpoints";
import type { OpportunityStage } from "@/api/models";
import { Page } from "@/components/layout";
import { usePaginatedData } from "@/components/table";
import { EmptyState, EnumFilter, LoadMore } from "@/components/ui";

import { SalesTable } from "../components/SalesTable";
import { SalesToolbar } from "../components/SalesToolbar";
import { salesStage } from "../types";

const SalesPage: React.FC = () => {
	const [stage, setStage] = useState<OpportunityStage | null>(null);
	const [search, setSearch] = useState<string>("");

	const fetchPage = useCallback(
		(page: number, pageSize: number) => {
			return getApiSalesOpportunity({
				page,
				pageSize,
				stage: stage ?? undefined,
				search: search,
			});
		},
		[stage, search],
	);

	const {
		data: opportunities,
		loading,
		hasMore,
		isEmpty,
		loadMore,
		refresh,
	} = usePaginatedData({
		pageSize: 15,
		fetchPage: fetchPage,
		queryKey: [stage, search],
	});

	return (
		<Page
			title="Sales"
			description="Manage your leads and sales opportunities THROUGH the pipeline."
			onRefresh={refresh}
			loading={loading}
			isEmpty={isEmpty}
			emptyState={
				<EmptyState title="No sales opportunities found">
					<DollarSign size={24} />
				</EmptyState>
			}
		>
			<SalesToolbar
				search={search}
				onSearchChange={(s) => setSearch(s)}
				onClear={() => {
					setSearch("");
				}}
				onAdd={() => {}}
			/>
			<EnumFilter
				value={stage}
				options={salesStage}
				onChange={(s) => {
					setStage(s);
				}}
			/>
			<SalesTable items={opportunities} />
			<LoadMore loading={loading} hasNext={hasMore} onClick={loadMore} />
		</Page>
	);
};

export default SalesPage;
