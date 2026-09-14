import { MessageSquare } from "lucide-react";
import type React from "react";

import { Route } from "#/routes/app/interviews";
import { Page } from "@/components/layout";
import { EmptyState, EnumFilter, LoadMore } from "@/components/ui";
import { interviewStatuses } from "../type";
import { InterviewsTable, InterviewToolbar } from "./components";
import { type InterviewsFilters, useGetInterviewsSlice } from "./hooks/useInterviews";

const InterviewsPage: React.FC = () => {
	const navigate = Route.useNavigate();
	const search = Route.useSearch() as InterviewsFilters;
	const query = useGetInterviewsSlice(search);

	const items = query.data?.pages.flatMap((page) => page.content ?? []) ?? [];
	const hasMore = query.data?.pages.flatMap((page) => page.hasMore ?? [false]) ?? [false];
	const isEmpty = query.isFetched && items.length === 0;

	return (
		<Page
			title="Interviews"
			description="Schedule and manage interviews with job applicants."
			onRefresh={() => query.refetch()}
			loading={query.isPending}
			isEmpty={isEmpty}
			emptyState={
				<EmptyState title="No interviews found">
					<MessageSquare size={24} />
				</EmptyState>
			}
		>
			<InterviewToolbar
				search={search.search ?? ""}
				onClear={() => {
					navigate({ search: {} });
				}}
				onSearchChange={(v) => {
					navigate({ search: (previous) => ({ ...previous, search: v }) });
				}}
			/>
			<EnumFilter
				value={search.status ?? null}
				options={interviewStatuses}
				onChange={(s) => {
					navigate({ search: (previous) => ({ ...previous, status: s }) });
				}}
			/>
			<InterviewsTable
				items={items}
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
	);
};

export default InterviewsPage;
