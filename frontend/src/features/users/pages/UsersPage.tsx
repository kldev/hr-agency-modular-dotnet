import { Users } from "lucide-react";
import type React from "react";
import { getApiUsers } from "@/api/endpoints";
import { Page } from "@/components/layout";
import { usePaginatedData } from "@/components/table";
import { EmptyState, LoadMore } from "@/components/ui";
import { UseresTable } from "../components/UseresTable";

const UsersPage: React.FC = () => {
	const {
		data: users,
		loading,
		hasMore,
		isEmpty,
		loadMore,
		refresh,
	} = usePaginatedData({
		pageSize: 15,
		fetchPage: (page, pageSize) => getApiUsers({ page, pageSize, roles: [] }),
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
			{!isEmpty ? (
				<>
					<UseresTable users={users} />
					<LoadMore loading={loading} hasNext={hasMore} onClick={loadMore} />
				</>
			) : null}
		</Page>
	);
};

export default UsersPage;
