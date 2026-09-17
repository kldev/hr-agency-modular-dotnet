import { Link } from "@tanstack/react-router";
import { BriefcaseBusiness } from "lucide-react";
import type React from "react";

import { Route } from "#/routes/app/jobs";
import { Page } from "@/components/layout";

import { EmptyState, EnumFilter, LoadMore } from "@/components/ui";
import { jobPostsStatuses } from "../type";
import { JobPostCardList, JobPostsTable, JobsPageToolbar } from "./components";
import { type JobsFilters, useGetJobsSlice } from "./hooks";

const JobsPage: React.FC = () => {
	const navigate = Route.useNavigate();
	const search = Route.useSearch() as JobsFilters;
	const query = useGetJobsSlice(search);
	const items = query.data?.pages.flatMap((page) => page.content ?? []) ?? [];
	const hasMore = query.data?.pages.flatMap((page) => page.hasMore ?? [false]) ?? [false];
	const isEmpty = query.isFetched && items.length === 0;
	return (
		<Page
			className="has-mobile-view"
			title="Job postings"
			description=" Manage job posts."
			onRefresh={() => query.refetch()}
			loading={query.isPending}
			isEmpty={isEmpty}
			emptyState={
				<EmptyState
					title="No job posts found"
					description="A job post is always created from a job description - there is no way to start one from this list."
				>
					<BriefcaseBusiness />

					<Link to="/app/job-descriptions">Go to job descriptions</Link>
				</EmptyState>
			}
		>
			<JobsPageToolbar
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
				options={jobPostsStatuses}
				onChange={(s) => {
					navigate({ search: (previous) => ({ ...previous, status: s }) });
				}}
			/>
			<JobPostsTable
				items={items}
				onRefresh={() => {
					query.refetch();
				}}
			/>
			<JobPostCardList jobPosts={items} onRefresh={() => query.refetch()} />
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

export default JobsPage;
