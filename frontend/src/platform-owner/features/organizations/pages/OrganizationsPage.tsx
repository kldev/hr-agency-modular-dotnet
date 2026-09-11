import { Globe2 } from "lucide-react";
import type React from "react";
import { useCallback, useState } from "react";
import { getApiOrganization } from "@/api/endpoints";
import { Page } from "@/components/layout";
import { usePaginatedData } from "@/components/table";
import { EmptyState, LoadMore } from "@/components/ui";
import { OrganizationsTable } from "./components/OrganizationsTable";
import { OrganizationsToolbar } from "./components/OrganizationsToolbar";

const OrganizationsPage: React.FC = () => {
	const [search, setSearch] = useState<string>("");
	const fetchPage = useCallback((page: number, pageSize: number) => {
		return getApiOrganization({
			page,
			pageSize,
			search: search ?? undefined
		});
	}, [search]);

	const {
		data: items,
		loading,
		hasMore,
		isEmpty,
		loadMore,
		refresh,
	} = usePaginatedData({
		pageSize: 15,
		fetchPage: fetchPage,
		queryKey: [search],
	});
	return (
		<Page
			title="Organizations"
			description="Manage organizations"
			onRefresh={refresh}
			loading={loading}
			isEmpty={isEmpty}
			emptyState={
				<EmptyState title="No organizations found">
					<Globe2 size={24} />
				</EmptyState>
			}
		>
			<OrganizationsToolbar
				search={search}
				onSearchChange={(s) => setSearch(s)}
				onClear={() => {
					setSearch("");
				}}
				onAdd={() => { }}
			/>
			<OrganizationsTable items={items} />
			<LoadMore loading={loading} hasNext={hasMore} onClick={loadMore} />
		</Page>
	);
};

export default OrganizationsPage;
