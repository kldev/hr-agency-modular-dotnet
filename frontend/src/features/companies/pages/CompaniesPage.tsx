import { Building2 } from "lucide-react";
import type React from "react";
import { useCallback, useRef, useState } from "react";
import { getCompanies } from "@/api/endpoints";
import { Page } from "@/components/layout";
import { usePaginatedData } from "@/components/table/usePaginatedData";
import { EmptyState, LoadMore } from "@/components/ui";
import { CompaniesTable, CraeteCompanyDrawer } from "./components";
import { CompaniesToolbar } from "./components/CompaniesToolbar";
import type { CreateCompanyFormCommand } from "./components/CompanyFormCommand";

const CompaniesPage: React.FC = () => {
	const [search, setSearch] = useState<string>("");
	const formRef = useRef<CreateCompanyFormCommand>(null);
	const fetchPage = useCallback(
		(page: number, pageSize: number) => {
			return getCompanies({
				page,
				pageSize,
				search,
			});
		},
		[search],
	);

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
		queryKey: [search],
	});

	return (
		<>
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
				<CompaniesToolbar
					onAdd={() => {
						formRef.current?.create();
					}}
					search={search}
					onClear={() => {
						setSearch("");
					}}
					onSearchChange={(s) => setSearch(s)}
				/>
				<CompaniesTable companies={companies} onRefresh={refresh} />
				<LoadMore loading={loading} hasNext={hasMore} onClick={loadMore} />
			</Page>
			<CraeteCompanyDrawer
				ref={formRef}
				onSuccess={() => {
					refresh();
				}}
			/>
		</>
	);
};

export default CompaniesPage;
