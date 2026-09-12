import { Users } from "lucide-react";
import type React from "react";
import { useCallback, useRef, useState } from "react";
import { getCandidates } from "@/api/endpoints";
import type { CandidateSource } from "@/api/models";
import { Page } from "@/components/layout";
import { usePaginatedData } from "@/components/table";
import { EmptyState, LoadMore } from "@/components/ui";
import {
	CandidatesTable,
	CandidatesToolbar,
	CreateCandidateDrawer,
	type CreateCandidateFormCommand,
} from "../components";

const CandidatesPage: React.FC = () => {
	const [source, setSource] = useState<CandidateSource | null>(null);
	const [search, setSearch] = useState<string>("");
	const formRef = useRef<CreateCandidateFormCommand>(null);

	const fetchPage = useCallback(
		(page: number, pageSize: number) => {
			return getCandidates({
				page,
				pageSize,
				...(source ? { source: [source] } : {}),
				search,
			});
		},
		[source, search],
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
		queryKey: [source, search],
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
			<CandidatesToolbar
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
				onAdd={() => {
					formRef.current?.create();
				}}
			/>
			<CandidatesTable items={items} onRefresh={refresh} />
			<LoadMore loading={loading} hasNext={hasMore} onClick={loadMore} />
			<CreateCandidateDrawer ref={formRef} onSuccess={refresh} />
		</Page>
	);
};

export default CandidatesPage;
