import { Users } from "lucide-react";
import type React from "react";
import { useMemo, useRef } from "react";

import { useGetOrgStructure } from "#/features/org-structure/pages/hooks";
import { unitByMemberId } from "#/features/org-structure/types";
import { Route } from "#/routes/app/users";
import { Page } from "@/components/layout";
import { EmptyState, EnumFilter, LoadMore } from "@/components/ui";
import {
	CreateUserDrawer,
	type CreateUserFormCommand,
	UseresTable,
	UsersCardList,
	UsersToolbar,
} from "../components";
import { organizationRoles } from "../types";
import { type UsersFilters, useGetUsersSlice } from "./hooks";

const UsersPage: React.FC = () => {
	const formRef = useRef<CreateUserFormCommand>(null);

	const navigate = Route.useNavigate();
	const search = Route.useSearch() as UsersFilters;

	const query = useGetUsersSlice(search);

	/*
	 * The chart answers "who sits where" for the unit column. It is one small document and it shares
	 * a cache key with the org structure page, so arriving from there costs nothing.
	 */
	const structure = useGetOrgStructure();

	const unitsByMember = useMemo(
		() => unitByMemberId(structure.data?.units ?? []),
		[structure.data],
	);

	const items = query.data?.pages.flatMap((page) => page.content ?? []) ?? [];
	const hasMore = query.data?.pages.flatMap((page) => page.hasMore ?? [false]) ?? [false];
	const isEmpty = query.isFetched && items.length === 0;

	return (
		<Page
			className="has-mobile-view"
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
			<UseresTable
				users={items}
				unitOf={(userId) => unitsByMember.get(userId)}
				onRefresh={() => query.refetch()}
			/>
			<UsersCardList items={items} />
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
