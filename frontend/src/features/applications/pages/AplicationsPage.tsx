import { useQueryClient } from "@tanstack/react-query";
import { ClipboardList } from "lucide-react";
import { Route } from "#/routes/app/applications";
import { applicationKeys } from "@/api/query-keys";
import { Page } from "@/components/layout";
import { EmptyState, EnumFilter, LoadMore } from "@/components/ui";
import { applicationStatuses } from "../types";
import {
	ApplicationCardList,
	ApplicationsKanban,
	ApplicationsTable,
	ApplicationsToolbar,
} from "./components";
import { useGetApplicationsSlice } from "./hooks";

const AplicationsPage: React.FC = () => {
	const navigate = Route.useNavigate();
	const search = Route.useSearch();
	const client = useQueryClient();

	const view = search.view ?? "table";
	const isTable = view === "table";

	const filters = { search: search.search, source: search.source, worker: search.worker };

	const applicationsQuery = useGetApplicationsSlice(
		{ ...filters, status: search.status },
		{ enabled: isTable },
	);

	const items = applicationsQuery.data?.pages.flatMap((page) => page.content ?? []) ?? [];
	const hasMore = applicationsQuery.data?.pages.flatMap((page) => page.hasMore ?? [false]) ?? [
		false,
	];

	// on the board a change moves a card between two columns, so every column is refetched
	const refresh = () => {
		if (isTable) {
			void applicationsQuery.refetch();
			return;
		}

		void client.invalidateQueries({ queryKey: applicationKeys.lists() });
	};

	return (
		<Page
			className="has-mobile-view"
			wide={!isTable}
			title="Applications"
			description="Track candidates through the recruitment process."
			onRefresh={refresh}
			loading={isTable && applicationsQuery.isPending}
			isEmpty={isTable && applicationsQuery.isFetched && items.length === 0}
			emptyState={
				<EmptyState title="No applications found">
					<ClipboardList size={24} />
				</EmptyState>
			}
		>
			<ApplicationsToolbar
				search={search.search ?? ""}
				onClear={() => {
					navigate({ search: (previous) => ({ view: previous.view }) });
				}}
				onSearchChange={(s) => navigate({ search: (previous) => ({ ...previous, search: s }) })}
				source={search.source ?? null}
				onSourceChange={(s) =>
					navigate({ search: (previous) => ({ ...previous, source: s ?? undefined }) })
				}
				worker={search.worker ?? null}
				onWorkerChange={(worker) =>
					navigate({ search: (previous) => ({ ...previous, worker: worker ?? undefined }) })
				}
				view={view}
				onViewChange={(next) =>
					navigate({
						search: (previous) => ({ ...previous, view: next === "table" ? undefined : next }),
					})
				}
			/>

			{isTable ? (
				<>
					<div className="flex-col">
						<EnumFilter
							value={search.status ?? null}
							options={applicationStatuses}
							onChange={(s) => {
								navigate({ search: (previous) => ({ ...previous, status: s ?? undefined }) });
							}}
						/>
					</div>

					<ApplicationsTable items={items} onRefresh={refresh} />
					<ApplicationCardList applications={items} onRefresh={refresh} />

					<LoadMore
						loading={applicationsQuery.isPending}
						hasNext={hasMore[0]}
						onClick={() => {
							applicationsQuery.fetchNextPage();
						}}
					/>
				</>
			) : (
				<ApplicationsKanban filters={filters} onSuccess={refresh} />
			)}
		</Page>
	);
};

export default AplicationsPage;
