import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import type React from "react";

const queryClient = new QueryClient({
	defaultOptions: {
		queries: {
			staleTime: 5 * 60 * 1000, // 5 minutes
			retry: (failureCount, error) => {
				// Don't retry on 401/403
				if (
					error &&
					typeof error === "object" &&
					"status" in error &&
					(error.status === 401 || error.status === 403)
				) {
					return false;
				}
				// Retry once for 5xx errors
				return failureCount < 1;
			},
			refetchOnWindowFocus: false,
		},
		mutations: {
			retry: false,
		},
	},
});

interface QueryProviderProps {
	children: React.ReactNode;
}

const QueryProvider: React.FC<QueryProviderProps> = ({ children }) => {
	return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;
};

export { queryClient };
export default QueryProvider;
