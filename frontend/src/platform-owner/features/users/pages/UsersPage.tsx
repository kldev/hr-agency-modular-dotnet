import { Users } from "lucide-react";
import type React from "react";
import { useCallback, useState } from "react";
import { getOrganizationsUsers } from "@/api/endpoints";
import type { OrganizationRoleApi } from "@/api/models";
import { Page } from "@/components/layout";
import { usePaginatedData } from "@/components/table";
import { EmptyState, EnumFilter, LoadMore } from "@/components/ui";
import { UsersToolbar } from "@/features/users/components/UsersToolbar";
import { organizationRoles } from "@/features/users/types";
import { UseresTable } from "./components/UseresTable";

const UsersPage: React.FC = () => {
	const [role, setRole] = useState<OrganizationRoleApi | null>(null);
	const [search, setSearch] = useState<string>("");
	const fetchPage = useCallback(
		(page: number, pageSize: number) => {
			return getOrganizationsUsers({
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
				onAdd={() => {}}
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
		</Page>
	);
};

export default UsersPage;
