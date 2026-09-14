import { Users } from "lucide-react";
import type React from "react";
import { Route } from "#/routes/admin/users";
import { Page } from "@/components/layout";
import { EmptyState, EnumFilter, LoadMore } from "@/components/ui";
import { UsersToolbar } from "@/features/users/components/UsersToolbar";
import { organizationRoles } from "@/features/users/types";
import { UseresTable } from "./components";
import { type UsersFilters, useGetOrganizationsUsersSlice } from "./hooks";

const UsersPage: React.FC = () => {
	const navigate = Route.useNavigate();
	const search = Route.useSearch() as UsersFilters;

	const query = useGetOrganizationsUsersSlice(search);
	const items = query.data?.pages.flatMap((page) => page.content ?? []) ?? [];
	const hasMore = query.data?.pages.flatMap((page) => page.hasMore ?? [false]) ?? [false];
	const isEmpty = query.isFetched && items.length === 0;

	return (
		<Page
			title="Users"
			description="People with access to the organization"
			onRefresh={() => query.refetch()}
			loading={query.isPending}
			isEmpty={isEmpty}
			emptyState={
				<EmptyState title="No users found">
					<Users size={24} />
				</EmptyState>
			}
		>
			<UsersToolbar
				search={search.search ?? ""}
				onClear={() => {
					navigate({ search: {} });
				}}
				onSearchChange={(v) => {
					navigate({ search: (previous) => ({ ...previous, search: v }) });
				}}
			/>
			<EnumFilter
				value={search.role ?? null}
				options={organizationRoles}
				onChange={(s) => {
					navigate({ search: (previous) => ({ ...previous, role: s }) });
				}}
			/>
			<UseresTable users={items} />
			<LoadMore
				loading={query.isPending}
				hasNext={hasMore[0]}
				onClick={() => {
					query.fetchNextPage();
				}}
			/>
		</Page>
	);
};

export default UsersPage;
