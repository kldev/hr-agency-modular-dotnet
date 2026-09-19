import axios, { type AxiosError, type AxiosRequestConfig } from "axios";
import type { BadRequestDetails } from "./models";

const api = axios.create({
	baseURL: "/",
	paramsSerializer: {
		indexes: null,
	},
});

/// A 401 from signing in, refreshing or signing out answers the credential just presented - it is
/// not a session that ran out, and bouncing would reload the page the user is typing on.
const CARRIES_OWN_CREDENTIAL = /^\/api\/auth\//;

api.interceptors.response.use(
	(response) => response,
	(error) => {
		if (
			axios.isAxiosError(error) &&
			error.response?.status === 401 &&
			!CARRIES_OWN_CREDENTIAL.test(error.config?.url ?? "")
		) {
			// The proxy already spent its one renewal on this call, so a 401 reaching the browser
			// means the session is really over. A full load rather than a router navigation: it
			// drops every stale cache and lets the server read the cleared cookies again. A thrown
			// `redirect` used to sit here, but nothing catches one raised inside a query.
			if (typeof window !== "undefined") {
				window.location.href = "/login";
			}

			return Promise.reject(error);
		}

		if (axios.isAxiosError(error) && error.response?.status === 400) {
			return Promise.reject(error.response.data as BadRequestDetails);
		}

		return Promise.reject(error);
	},
);

export const customInstance = <T>(
	config: AxiosRequestConfig,
	options?: AxiosRequestConfig,
): Promise<T> => {
	return api({
		...config,
		...options,
	}).then(({ data }) => data);
};

export type ErrorType<Error> = AxiosError<Error>;
export type BodyType<BodyData> = BodyData;
