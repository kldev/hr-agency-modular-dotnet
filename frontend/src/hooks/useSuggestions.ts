import { useQuery } from "@tanstack/react-query";
import {
	getCompanySuggestion,
	getProjectSuggestion,
	getTeamSuggestion,
	getUserSuggestion,
	getWorkerSuggestion,
} from "#/api/endpoints";
import { suggestionKeys } from "#/api/query-keys";

const SUGGESTION_STALE_TIME = 5 * 60 * 1000;

/*
 * Reads a single suggestion by id - the same query keys the pickers fill, so a company already
 * resolved by CompaniesPicker is served from the cache instead of a second request.
 */

export function useCompanySuggestion(companyId: string) {
	return useQuery({
		queryKey: suggestionKeys.company(companyId),
		queryFn: ({ signal }) => getCompanySuggestion(companyId, undefined, signal),
		enabled: Boolean(companyId),
		staleTime: SUGGESTION_STALE_TIME,
		retry: false,
	});
}

export function useUserSuggestion(userId: string) {
	return useQuery({
		queryKey: suggestionKeys.user(userId),
		queryFn: ({ signal }) => getUserSuggestion(userId, undefined, signal),
		enabled: Boolean(userId),
		staleTime: SUGGESTION_STALE_TIME,
		retry: false,
	});
}

export function useTeamSuggestion(teamId: string) {
	return useQuery({
		queryKey: suggestionKeys.team(teamId),
		queryFn: ({ signal }) => getTeamSuggestion(teamId, undefined, signal),
		enabled: Boolean(teamId),
		staleTime: SUGGESTION_STALE_TIME,
		retry: false,
	});
}

export function useWorkerSuggestion(workerId: string) {
	return useQuery({
		queryKey: suggestionKeys.worker(workerId),
		queryFn: ({ signal }) => getWorkerSuggestion(workerId, undefined, signal),
		enabled: Boolean(workerId),
		staleTime: SUGGESTION_STALE_TIME,
		retry: false,
	});
}

export function useProjectSuggestion(projectId: string) {
	return useQuery({
		queryKey: suggestionKeys.project(projectId),
		queryFn: ({ signal }) => getProjectSuggestion(projectId, undefined, signal),
		enabled: Boolean(projectId),
		staleTime: SUGGESTION_STALE_TIME,
		retry: false,
	});
}
