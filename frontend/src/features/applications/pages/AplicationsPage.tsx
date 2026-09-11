import { ClipboardList } from "lucide-react";
import { useCallback, useState } from "react";
import { getApiRecruitmentJobApplications } from "@/api/endpoints";
import type { CandidateSource, JobApplicationStatus } from "@/api/models";
import { Page } from "@/components/layout";
import { usePaginatedData } from "@/components/table";
import { EmptyState, EnumFilter, LoadMore } from "@/components/ui";
import { applicationStatuses } from "../types";
import { AplicationsTable } from "./components/AplicationsTable";
import { ApplicationsToolbar } from "./components/ApplicationsToolbar";

const AplicationsPage: React.FC = () => {
	const [status, setStatus] = useState<JobApplicationStatus | null>(null);
	const [source, setSource] = useState<CandidateSource | null>(null);
	const [search, setSearch] = useState<string>("");

	const fetchPage = useCallback(
		(page: number, pageSize: number) => {
			return getApiRecruitmentJobApplications({
				page,
				pageSize,
				...(status ? { status: [status] } : {}),
				search: search,
				...(source ? { source: [source] } : {}),
			});
		},
		[status, source, search],
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
		queryKey: [status, source, search],
	});

	return (
		<Page
			title="Applications"
			description="Track candidates through the recruitment process."
			onRefresh={() => refresh()}
			loading={loading}
			isEmpty={isEmpty}
			emptyState={
				<EmptyState title="No applications found">
					<ClipboardList size={24} />
				</EmptyState>
			}
		>
			<ApplicationsToolbar
				onClear={() => {
					setSearch("");
					setSource(null);
				}}
				search={search}
				onSearchChange={(s) => setSearch(s)}
				source={source}
				onSourceChange={(s) => {
					setSource(s);
				}}
				onAdd={() => {}}
			/>

			<div className="flex-col">
				<EnumFilter value={status} options={applicationStatuses} onChange={setStatus} />
			</div>

			<AplicationsTable items={items} />
			<LoadMore loading={loading} hasNext={hasMore} onClick={loadMore} />
		</Page>
	);
};

export default AplicationsPage;
