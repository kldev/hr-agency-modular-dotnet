import { Users } from "lucide-react";
import type React from "react";
import { useCallback } from "react";
import { getApiUsers } from "@/api/endpoints";
import { Page } from "@/components/layout";
import { usePaginatedData } from "@/components/table";
import { EmptyState, LoadMore } from "@/components/ui";
import { UseresTable } from "../components/UseresTable";

const UsersPage: React.FC = () => {
	const fetchPage = useCallback((page: number, pageSize: number) => {
		return getApiUsers({
			page,
			pageSize,
			roles: [],
		});
	}, []);

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
		queryKey: []
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
			<UseresTable users={users} />
			<LoadMore loading={loading} hasNext={hasMore} onClick={loadMore} />
		</Page>
	);
};

export default UsersPage;
