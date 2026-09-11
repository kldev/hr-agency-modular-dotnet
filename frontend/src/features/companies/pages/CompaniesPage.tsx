import { Building2 } from "lucide-react";
import type React from "react";
import { useCallback } from "react";
import { getApiCompanies } from "@/api/endpoints";
import { Page } from "@/components/layout";
import { usePaginatedData } from "@/components/table/usePaginatedData";
import { EmptyState, LoadMore } from "@/components/ui";
import { CompaniesTable } from "./components/CompaniesTable";

const CompaniesPage: React.FC = () => {
	const fetchPage = useCallback((page: number, pageSize: number) => {
		return getApiCompanies({
			page,
			pageSize,
		});
	}, []);

	const {
		data: companies,
		loading,
		hasMore,
		isEmpty,
		loadMore,
		refresh,
	} = usePaginatedData({
		pageSize: 15,
		fetchPage: fetchPage,
	});

	return (
		<Page
			title="Companies"
			description="Manage companies and their recruitment relationships."
			onRefresh={refresh}
			loading={loading}
			page={0}
			isEmpty={isEmpty}
			emptyState={
				<EmptyState title="No companies found">
					<Building2 size={24} />
				</EmptyState>
			}
		>
			<CompaniesTable companies={companies} />
			<LoadMore loading={loading} hasNext={hasMore} onClick={loadMore} />
		</Page>
	);
};

export default CompaniesPage;
