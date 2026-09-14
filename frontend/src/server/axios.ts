import { getCookie } from "@tanstack/react-start/server";
import type { customInstance } from "#/api/mutator";
import { API_URL } from "#/routes/api/$";

type SecondParameter<T extends (...args: never) => unknown> = Parameters<T>[1];

/// for server FN only
export function getFnOptions(): SecondParameter<typeof customInstance> {
	const token = getCookie("access_token") as string;

	return {
		paramsSerializer: {
			indexes: null,
		},
		headers: { Authorization: `Bearer ${token}`, "Content-Type": "application/json" },
		baseURL: API_URL,
	};
}
