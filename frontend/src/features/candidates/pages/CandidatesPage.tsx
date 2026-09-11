import { Users } from "lucide-react";
import type React from "react";
import { useCallback } from "react";
import { getApiRecruitmentCandidates } from "@/api/endpoints";
import { Page } from "@/components/layout";
import { usePaginatedData } from "@/components/table";
import { EmptyState, LoadMore } from "@/components/ui";
import { CandidatesTable } from "../components/CandidatesTable";

const CandidatesPage: React.FC = () => {
	const fetchPage = useCallback((page: number, pageSize: number) => {
		return getApiRecruitmentCandidates({
			page,
			pageSize,
		});
	}, []);

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
	});

	return (
		<Page
			title="Candidates"
			description="Manage candidates and their recruitment profiles."
			onRefresh={refresh}
			loading={loading}
			isEmpty={isEmpty}
			emptyState={
				<EmptyState title="No candidates found">
					<Users size={24} />
				</EmptyState>
			}
		>
			{!isEmpty ? (
				<>
					<CandidatesTable items={items} />
					<LoadMore loading={loading} hasNext={hasMore} onClick={loadMore} />
				</>
			) : null}
		</Page>
	);
};

export default CandidatesPage;
