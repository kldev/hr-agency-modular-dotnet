import { ChessRook } from "lucide-react";
import type React from "react";

import { Route } from "#/routes/app/job-descriptions";

import { Page } from "@/components/layout";

import { EmptyState, EnumFilter, LoadMore } from "@/components/ui";
import { jobDescriptionStatuses } from "../type";
import { JobsDescriptopnTable, JobsDescriptopnToolbar } from "./components";
import { type JobDescriptionPageFillters, useGetJobDescriptionSlice } from "./hooks";

const JobDescriptionPage: React.FC = () => {
	const navigate = Route.useNavigate();
	const search = Route.useSearch() as JobDescriptionPageFillters;
	const query = useGetJobDescriptionSlice(search);
	const items = query.data?.pages.flatMap((page) => page.content ?? []) ?? [];
	const hasMore = query.data?.pages.flatMap((page) => page.hasMore ?? [false]) ?? [false];
	const isEmpty = query.isFetched && items.length === 0;
	const onRefresh = () => {
		query.refetch();
	};

	return (
		<Page
			title="Jobs description"
			description=" Manage jobs description."
			onRefresh={onRefresh}
			loading={query.isPending}
			isEmpty={isEmpty}
			emptyState={
				<EmptyState title="No job description found">
					<ChessRook />
				</EmptyState>
			}
		>
			<JobsDescriptopnToolbar
				search={search.search ?? ""}
				onClear={() => {
					navigate({ search: {} });
				}}
				onSearchChange={(v) => {
					navigate({ search: (previous) => ({ ...previous, search: v }) });
				}}
			/>
			<EnumFilter
				value={search.status || null}
				options={jobDescriptionStatuses}
				onChange={(s) => {
					navigate({ search: (previous) => ({ ...previous, status: s }) });
				}}
			/>
			<JobsDescriptopnTable items={items} onRefresh={onRefresh} />
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

export default JobDescriptionPage;
