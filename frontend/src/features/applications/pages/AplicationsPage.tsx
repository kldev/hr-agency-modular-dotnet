import { ClipboardList } from "lucide-react";
import { Route } from "#/routes/app/applications";
import type { CandidateSource, JobApplicationStatus } from "@/api/models";
import { Page } from "@/components/layout";
import { EmptyState, EnumFilter, LoadMore } from "@/components/ui";
import { applicationStatuses } from "../types";
import { ApplicationCardList, ApplicationsTable, ApplicationsToolbar } from "./components";
import { useGetApplicationsSlice } from "./hooks";

export interface ApplicationFilters {
	status?: JobApplicationStatus;
	source?: CandidateSource;
	search?: string;
}

const AplicationsPage: React.FC = () => {
	const navigate = Route.useNavigate();
	const search = Route.useSearch() as ApplicationFilters;
	const applicationsQuery = useGetApplicationsSlice(search);

	const items = applicationsQuery.data?.pages.flatMap((page) => page.content ?? []) ?? [];
	const hasMore = applicationsQuery.data?.pages.flatMap((page) => page.hasMore ?? [false]) ?? [
		false,
	];

	return (
		<Page
			className="has-mobile-view"
			title="Applications"
			description="Track candidates through the recruitment process."
			onRefresh={() => applicationsQuery.refetch()}
			loading={applicationsQuery.isPending}
			isEmpty={applicationsQuery.isFetched && items.length === 0}
			emptyState={
				<EmptyState title="No applications found">
					<ClipboardList size={24} />
				</EmptyState>
			}
		>
			<ApplicationsToolbar
				onClear={() => {
					navigate({ search: {} });
				}}
				search={search.search ?? ""}
				onSearchChange={(s) => navigate({ search: { ...search, search: s } })}
				source={search.source ?? null}
				onSourceChange={(s) => navigate({ search: (previous) => ({ ...previous, source: s }) })}
			/>

			<div className="flex-col">
				<EnumFilter
					value={search.status ?? null}
					options={applicationStatuses}
					onChange={(s) => {
						navigate({ search: (previous) => ({ ...previous, status: s }) });
					}}
				/>
			</div>

			<ApplicationsTable items={items} onRefresh={() => applicationsQuery.refetch()} />
			<ApplicationCardList applications={items} />

			<LoadMore
				loading={applicationsQuery.isPending}
				hasNext={hasMore[0]}
				onClick={() => {
					applicationsQuery.fetchNextPage();
				}}
			/>
		</Page>
	);
};

export default AplicationsPage;
