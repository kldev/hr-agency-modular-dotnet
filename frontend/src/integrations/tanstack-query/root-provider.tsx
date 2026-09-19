import { QueryClient } from "@tanstack/react-query";
import axios from "axios";

export function getContext() {
	const queryClient = new QueryClient({
		defaultOptions: {
			queries: {
				// Retrying a rejected session only delays the redirect the interceptor is already
				// performing, and repeats a call that cannot start succeeding.
				retry: (failureCount, error) => !isAuthFailure(error) && failureCount < 1,
			},
			mutations: {
				retry: false,
			},
		},
	});

	return {
		queryClient,
	};
}

function isAuthFailure(error: unknown) {
	const status = axios.isAxiosError(error)
		? error.response?.status
		: (error as { status?: number } | null)?.status;

	return status === 401 || status === 403;
}

export default function TanstackQueryProvider() {}
