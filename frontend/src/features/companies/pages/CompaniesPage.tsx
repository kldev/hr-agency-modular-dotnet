import { Building2 } from "lucide-react";
import type React from "react";
import { useRef } from "react";
import { Route } from "#/routes/app/companies";
import { Page } from "@/components/layout";
import { EmptyState, LoadMore } from "@/components/ui";
import { CompaniesTable, CraeteCompanyDrawer, type CreateCompanyFormCommand } from "./components";
import { CompaniesToolbar } from "./components/CompaniesToolbar";
import { useGetCompaniesSlice } from "./hooks";

const CompaniesPage: React.FC = () => {
	const formRef = useRef<CreateCompanyFormCommand>(null);
	const navigate = Route.useNavigate();
	const search = Route.useSearch() as { search: string };

	const query = useGetCompaniesSlice(search.search);
	const items = query.data?.pages.flatMap((page) => page.content ?? []) ?? [];
	const hasMore = query.data?.pages.flatMap((page) => page.hasMore ?? [false]) ?? [false];
	const isEmpty = query.isFetched && items.length === 0;
	return (
		<>
			<Page
				title="Companies"
				description="Manage companies and their recruitment relationships."
				onRefresh={() => query.refetch()}
				loading={query.isPending}
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
					search={search.search ?? ""}
					onClear={() => {
						navigate({ search: {} });
					}}
					onSearchChange={(v) => {
						navigate({ search: (previous) => ({ ...previous, search: v }) });
					}}
				/>
				<CompaniesTable
					companies={items}
					onRefresh={() => {
						query.refetch();
					}}
				/>
				<LoadMore
					loading={query.isPending}
					hasNext={hasMore[0]}
					onClick={() => {
						query.fetchNextPage();
					}}
				/>
			</Page>
			<CraeteCompanyDrawer
				ref={formRef}
				onSuccess={() => {
					query.refetch();
				}}
			/>
		</>
	);
};

export default CompaniesPage;
