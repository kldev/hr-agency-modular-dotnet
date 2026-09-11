import { useCallback, useEffect, useState } from "react";

type PaginatedResponse<T> = {
	content?: T[];
	hasMore: boolean;
};

type UsePaginatedDataOptions<T> = {
	pageSize?: number;
	fetchPage: (page: number, pageSize: number) => Promise<PaginatedResponse<T>>;
};

type UsePaginatedDataResult<T> = {
	data: T[];
	loading: boolean;
	hasMore: boolean;
	isEmpty: boolean;
	loadMore: () => void;
	refresh: () => void;
};

export function usePaginatedData<T>({
	pageSize = 15,
	fetchPage,
}: UsePaginatedDataOptions<T>): UsePaginatedDataResult<T> {
	const [data, setData] = useState<T[]>([]);
	const [page, setPage] = useState(1);
	const [loading, setLoading] = useState(false);
	const [hasMore, setHasMore] = useState(false);
	const [initialized, setInitialized] = useState(false);

	const loadData = useCallback(
		async (pageNumber: number) => {
			setLoading(true);

			try {
				const response = await fetchPage(pageNumber, pageSize);
				const items = response.content ?? [];

				setData((current) => (pageNumber === 1 ? items : [...current, ...items]));

				setHasMore(response.hasMore);
				setInitialized(true);
			} finally {
				setLoading(false);
			}
		},
		[pageSize],
	);

	useEffect(() => {
		//console.log(`page changed ${page}`)
		if (page > 0) {
			void loadData(page);
		}
	}, [page, loadData]);

	const loadMore = useCallback(() => {
		if (loading || !hasMore) {
			return;
		}

		setPage((current) => current + 1);
	}, [loading, hasMore]);

	const refresh = useCallback(() => {
		setData([]);
		setHasMore(false);
		setInitialized(false);

		if (page === 1) void loadData(page);
		else {
			setPage(1);
		}
	}, [page]);

	const isEmpty = initialized && !loading && data.length === 0;

	return {
		data,
		loading,
		hasMore,
		isEmpty,
		loadMore,
		refresh,
	};
}
