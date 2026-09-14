import { Users } from "lucide-react";
import type React from "react";
import { useRef } from "react";

import { Route } from "#/routes/app/users";
import { Page } from "@/components/layout";
import { EmptyState, EnumFilter, LoadMore } from "@/components/ui";
import {
	CreateUserDrawer,
	type CreateUserFormCommand,
	UseresTable,
	UsersToolbar,
} from "../components";
import { organizationRoles } from "../types";
import { type UsersFilters, useGetUsersSlice } from "./hooks";

const UsersPage: React.FC = () => {
	const formRef = useRef<CreateUserFormCommand>(null);

	const navigate = Route.useNavigate();
	const search = Route.useSearch() as UsersFilters;

	const query = useGetUsersSlice(search);
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
				onAdd={() => {
					formRef.current?.create();
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
			<CreateUserDrawer
				ref={formRef}
				onSuccess={() => {
					query.refetch();
				}}
			/>
		</Page>
	);
};

export default UsersPage;
