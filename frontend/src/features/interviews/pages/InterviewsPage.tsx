import { MessageSquare } from "lucide-react";
import type React from "react";
import { useCallback, useState } from "react";
import { getApiInterviews } from "@/api/endpoints";
import type { InterviewStatus } from "@/api/models";
import { Page } from "@/components/layout";
import { usePaginatedData } from "@/components/table";
import { EmptyState, EnumFilter, LoadMore } from "@/components/ui";
import { interviewStatuses } from "../type";
import { InterviewsTable } from "./components/InterviewsTable";
import { InterviewToolbar } from "./components/InterviewToolbar";

const InterviewsPage: React.FC = () => {
	const [status, setStatus] = useState<InterviewStatus | null>(null);
	const [search, setSearch] = useState<string>("");

	const fetchPage = useCallback(
		(page: number, pageSize: number) => {
			return getApiInterviews({
				page,
				pageSize,
				status: status || undefined,
				search: search,
			});
		},
		[status, search],
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
		fetchPage,
		queryKey: [status, search],
	});
	return (
		<Page
			title="Interviews"
			description="Schedule and manage interviews with job applicants."
			onRefresh={refresh}
			loading={loading}
			isEmpty={isEmpty}
			emptyState={
				<EmptyState title="No interviews found">
					<MessageSquare size={24} />
				</EmptyState>
			}
		>
			<InterviewToolbar
				search={search}
				onClear={() => {
					setSearch("");
				}}
				onSearchChange={(s) => {
					setSearch(s);
				}}
			/>
			<EnumFilter value={status} options={interviewStatuses} onChange={setStatus} />
			<InterviewsTable items={items} />
			<LoadMore loading={loading} hasNext={hasMore} onClick={loadMore} />
		</Page>
	);
};

export default InterviewsPage;
