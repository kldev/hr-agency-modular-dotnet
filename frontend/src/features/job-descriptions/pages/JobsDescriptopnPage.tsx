import { ChessRook } from "lucide-react";
import type React from "react";
import { useCallback, useState } from "react";
import { getJobDescriptionsSlice } from "@/api/endpoints";
import type { JobDescriptionStatus } from "@/api/models";
import { Page } from "@/components/layout";
import { usePaginatedData } from "@/components/table";
import { EmptyState, EnumFilter, LoadMore } from "@/components/ui";
import { jobDescriptionStatuses } from "../type";
import { JobsDescriptopnTable, JobsDescriptopnToolbar } from "./components";

const JobsDescriptopnPage: React.FC = () => {
	const [search, setSearch] = useState("");
	const [status, setStatus] = useState<JobDescriptionStatus | null>(null);

	const fetchPage = useCallback(
		(page: number, pageSize: number) => {
			return getJobDescriptionsSlice({
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
			title="Jobs description"
			description=" Manage jobs description."
			onRefresh={refresh}
			loading={loading}
			isEmpty={isEmpty}
			emptyState={
				<EmptyState title="No job description found">
					<ChessRook />
				</EmptyState>
			}
		>
			<JobsDescriptopnToolbar
				search={search}
				onClear={() => {
					setSearch("");
				}}
				onSearchChange={(v) => {
					setSearch(v);
				}}
			/>
			<EnumFilter value={status} options={jobDescriptionStatuses} onChange={setStatus} />
			<JobsDescriptopnTable items={items} />
			<LoadMore loading={loading} hasNext={hasMore} onClick={loadMore} />
		</Page>
	);
};

export default JobsDescriptopnPage;
