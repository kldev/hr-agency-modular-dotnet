import type { customInstance } from "#/api/mutator";
import { API_URL } from "#/server/apiUrl";
import { currentAccessToken } from "#/server/session";

type SecondParameter<T extends (...args: never) => unknown> = Parameters<T>[1];

/// for server FN only
export async function getFnOptions(): Promise<SecondParameter<typeof customInstance>> {
	// Asynchronous because reading the token may first have to renew it; these calls talk to the API
	// directly, so nothing else along the way would notice an expired one.
	const token = await currentAccessToken();

	return {
		paramsSerializer: {
			indexes: null,
		},
		headers: { Authorization: `Bearer ${token}`, "Content-Type": "application/json" },
		baseURL: API_URL,
	};
}
