import { useCallback, useEffect, useState } from "react";

type PaginatedResponse<T> = {
	content?: T[];
	hasMore: boolean;
};

type UsePaginatedDataOptions<T> = {
	pageSize?: number;
	fetchPage: (page: number, pageSize: number) => Promise<PaginatedResponse<T>>;
	queryKey: readonly unknown[];
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
	queryKey,
}: UsePaginatedDataOptions<T>): UsePaginatedDataResult<T> {
	const [data, setData] = useState<T[]>([]);
	const [page, setPage] = useState(1);
	const [loading, setLoading] = useState(false);
	const [hasMore, setHasMore] = useState(false);
	const [initialized, setInitialized] = useState(false);

	const queryHash = JSON.stringify(queryKey);

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
		[fetchPage, pageSize],
	);

	useEffect(() => {
		setData([]);
		setHasMore(false);
		setInitialized(false);
		setPage(1);
	}, [queryHash]);

	useEffect(() => {
		void loadData(page);
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

		setPage((current) => {
			if (current === 1) {
				void loadData(1);
				return current;
			}

			return 1;
		});
	}, [loadData]);

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
