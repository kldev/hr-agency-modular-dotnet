import { Globe2 } from "lucide-react";
import type React from "react";
import { useRef } from "react";
import { Route } from "#/routes/admin/organizations";
import { Page } from "@/components/layout";
import { EmptyState, LoadMore } from "@/components/ui";
import {
	type CreateOrganizationCommand,
	CreateOrganizationDrawer,
	OrganizationsCardList,
	OrganizationsTable,
	OrganizationsToolbar,
} from "./components";
import { useGetOrganizationsSlice } from "./hooks";

const OrganizationsPage: React.FC = () => {
	const navigate = Route.useNavigate();
	const formRef = useRef<CreateOrganizationCommand>(null);
	const search = Route.useSearch() as { search?: string };
	const query = useGetOrganizationsSlice(search.search);
	const items = query.data?.pages.flatMap((page) => page.content ?? []) ?? [];
	const hasMore = query.data?.pages.flatMap((page) => page.hasMore ?? [false]) ?? [false];
	const isEmpty = query.isFetched && items.length === 0;
	const onRefresh = () => {
		query.refetch();
	};
	return (
		<Page
			className="has-mobile-view"
			title="Organizations"
			description="Manage organizations"
			onRefresh={onRefresh}
			loading={query.isPending}
			isEmpty={isEmpty}
			emptyState={
				<EmptyState title="No organizations found">
					<Globe2 size={24} />
				</EmptyState>
			}
		>
			<OrganizationsToolbar
				search={search.search ?? ""}
				onClear={() => {
					navigate({ search: {} });
				}}
				onSearchChange={(v) => {
					navigate({ search: (previous) => ({ ...previous, search: v }) });
				}}
				onAdd={() => {
					formRef.current?.create();
				}}
			/>
			<OrganizationsTable items={items} onRefresh={onRefresh} />
			<OrganizationsCardList items={items} />
			<LoadMore
				loading={query.isPending}
				hasNext={hasMore[0]}
				onClick={() => {
					query.fetchNextPage();
				}}
			/>
			<CreateOrganizationDrawer ref={formRef} onSuccess={onRefresh} />
		</Page>
	);
};

export default OrganizationsPage;
