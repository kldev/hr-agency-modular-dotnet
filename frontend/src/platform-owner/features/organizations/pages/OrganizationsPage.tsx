import { Globe2 } from "lucide-react";
import type React from "react";
import { useCallback, useRef, useState } from "react";
import { getOrganizations } from "@/api/endpoints";
import { Page } from "@/components/layout";
import { usePaginatedData } from "@/components/table";
import { EmptyState, LoadMore } from "@/components/ui";
import {
	type CreateOrganizationCommand,
	OrganizationsTable,
	OrganizationsToolbar,
} from "./components";

const OrganizationsPage: React.FC = () => {
	const formRef = useRef<CreateOrganizationCommand>(null);
	const [search, setSearch] = useState<string>("");
	const fetchPage = useCallback(
		(page: number, pageSize: number) => {
			return getOrganizations({
				page,
				pageSize,
				search: search ?? undefined,
			});
		},
		[search],
	);

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
				onAdd={() => {
					formRef.current?.create();
				}}
			/>
			<OrganizationsTable items={items} />
			<LoadMore loading={loading} hasNext={hasMore} onClick={loadMore} />
		</Page>
	);
};

export default OrganizationsPage;
