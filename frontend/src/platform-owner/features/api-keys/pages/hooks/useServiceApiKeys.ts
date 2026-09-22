import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { createServerFn } from "@tanstack/react-start";
import { serviceApiKeysKeys } from "#/api";
import { issueServiceAPIKey, listServiceAPIKeys, revokeServiceAPIKey } from "#/api/endpoints";
import { getFnOptions } from "#/server/axios";

const listServerFn = createServerFn({ method: "GET" }).handler(async () =>
	listServiceAPIKeys(await getFnOptions()),
);

export function useServiceApiKeys() {
	return useQuery({
		queryKey: serviceApiKeysKeys.list(),
		queryFn: () => listServerFn(),
	});
}

/**
 * Keys are plain documents written in the request's own transaction - no projection, so the list
 * can be invalidated straight away instead of waiting for a read model.
 */
export function useIssueServiceApiKey() {
	const client = useQueryClient();

	return useMutation({
		mutationFn: (name: string) => issueServiceAPIKey({ name }),
		onSuccess: () => client.invalidateQueries({ queryKey: serviceApiKeysKeys.all }),
	});
}

export function useRevokeServiceApiKey() {
	const client = useQueryClient();

	return useMutation({
		mutationFn: (keyId: string) => revokeServiceAPIKey(keyId),
		onSuccess: () => client.invalidateQueries({ queryKey: serviceApiKeysKeys.all }),
	});
}
