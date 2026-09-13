import { BriefcaseBusiness } from "lucide-react";
import type React from "react";
import { useCallback, useState } from "react";
import { getJobPostsSlice } from "@/api/endpoints";
import type { JobPostStatus } from "@/api/models";
import { Page } from "@/components/layout";
import { usePaginatedData } from "@/components/table";
import { EmptyState, EnumFilter, LoadMore } from "@/components/ui";
import { jobPostsStatuses } from "../type";
import { JobPostsTable, JobsPageToolbar } from "./components";

const JobsPage: React.FC = () => {
	const [search, setSearch] = useState("");
	const [status, setStatus] = useState<JobPostStatus | null>(null);
	const fetchPage = useCallback(
		(page: number, pageSize: number) => {
			return getJobPostsSlice({
				page,
				pageSize,
				...(status ? { status: [status] } : {}),
				search: search ?? undefined,
			});
		},
		[search, status],
	);

	const {
		data: items,
		loading,
		hasMore,
		isEmpty,
		loadMore,
		refresh,
	} = usePaginatedData({
		pageSize: 15,
		fetchPage: fetchPage,
		queryKey: [search, status],
	});
	return (
		<Page
			title="Job postings"
			description=" Manage job posts."
			onRefresh={refresh}
			loading={loading}
			isEmpty={isEmpty}
			emptyState={
				<EmptyState title="No job posts found">
					<BriefcaseBusiness />
				</EmptyState>
			}
		>
			<JobsPageToolbar
				onClear={() => {}}
				search={search}
				onSearchChange={(v) => {
					setSearch(v);
				}}
			/>

			<EnumFilter value={status} options={jobPostsStatuses} onChange={setStatus} />
			<JobPostsTable items={items} onRefresh={refresh} />
			<LoadMore loading={loading} hasNext={hasMore} onClick={loadMore} />
		</Page>
	);
};

export default JobsPage;
