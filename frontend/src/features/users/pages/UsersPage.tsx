import { Users } from "lucide-react";
import type React from "react";
import { useCallback, useRef, useState } from "react";
import { getUsers } from "@/api/endpoints";
import type { OrganizationRoleApi } from "@/api/models";
import { Page } from "@/components/layout";
import { usePaginatedData } from "@/components/table";
import { EmptyState, EnumFilter, LoadMore } from "@/components/ui";
import {
	CreateUserDrawer,
	type CreateUserFormCommand,
	UseresTable,
	UsersToolbar,
} from "../components";

import { organizationRoles } from "../types";

const UsersPage: React.FC = () => {
	const formRef = useRef<CreateUserFormCommand>(null);
	const [role, setRole] = useState<OrganizationRoleApi | null>(null);
	const [search, setSearch] = useState<string>("");
	const fetchPage = useCallback(
		(page: number, pageSize: number) => {
			return getUsers({
				page,
				pageSize,
				search: search,
				...(role ? { roles: [role] } : { roles: [] }),
			});
		},
		[search, role],
	);

	const {
		data: users,
		loading,
		hasMore,
		isEmpty,
		loadMore,
		refresh,
	} = usePaginatedData({
		pageSize: 15,
		fetchPage: fetchPage,
		queryKey: [search, role],
	});

	return (
		<Page
			title="Users"
			description="People with access to the organization"
			onRefresh={refresh}
			loading={loading}
			page={0}
			isEmpty={isEmpty}
			emptyState={
				<EmptyState title="No users found">
					<Users size={24} />
				</EmptyState>
			}
		>
			<UsersToolbar
				search={search}
				onSearchChange={(s) => setSearch(s)}
				onClear={() => {
					setSearch("");
				}}
				onAdd={() => {
					formRef.current?.create();
				}}
			/>
			<EnumFilter
				value={role}
				options={organizationRoles}
				onChange={(s) => {
					setRole(s);
				}}
			/>
			<UseresTable users={users} />
			<LoadMore loading={loading} hasNext={hasMore} onClick={loadMore} />
			<CreateUserDrawer ref={formRef} onSuccess={refresh} />
		</Page>
	);
};

export default UsersPage;
