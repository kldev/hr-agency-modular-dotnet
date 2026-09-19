import { createFileRoute } from "@tanstack/react-router";
import axios from "axios";
import { API_URL } from "#/server/apiUrl";
import { currentAccessToken, refreshSession } from "#/server/session";

export const Route = createFileRoute("/api/$")({
	server: {
		handlers: {
			GET: async ({ request }) => {
				return proxyRequest(request);
			},
			POST: async ({ request }) => {
				return proxyRequest(request);
			},
			PUT: async ({ request }) => {
				return proxyRequest(request);
			},
			PATCH: async ({ request }) => {
				return proxyRequest(request);
			},
			DELETE: async ({ request }) => {
				return proxyRequest(request);
			},
		},
	},
});

/// Signing in, refreshing and signing out carry their own credential in the body. Attaching a
/// bearer token to them - or renewing one on their behalf - would at best be pointless and at worst
/// loop the proxy through the refresh endpoint it is trying to call.
const CARRIES_OWN_CREDENTIAL = /^\/api\/auth\//;

async function proxyRequest(request: Request) {
	const url = new URL(request.url);
	const anonymous = CARRIES_OWN_CREDENTIAL.test(url.pathname);

	const targetUrl = `${API_URL}/api${url.pathname.replace(/^\/api/, "")}${url.search}`;

	const headers: Record<string, string> = {};

	request.headers.forEach((value, key) => {
		if (key !== "host" && key !== "content-length") {
			headers[key] = value;
		}
	});

	// Read once and keep it: a retry after a renewal needs the same body, and the request stream is
	// only good for a single read.
	const body =
		request.method === "GET" || request.method === "HEAD" ? undefined : await request.arrayBuffer();

	try {
		let response = await forward(targetUrl, request.method, headers, body, {
			token: anonymous ? undefined : await currentAccessToken(),
		});

		if (response.status === 401 && !anonymous) {
			// The token was accepted as fresh but the API disagrees - a revoked session, a restarted
			// API, a clock apart. Worth exactly one renewal before giving the 401 to the browser.
			const renewed = await refreshSession();

			if (renewed) {
				response = await forward(targetUrl, request.method, headers, body, { token: renewed });
			}
		}

		const responseHeaders = new Headers();

		for (const [key, value] of Object.entries(response.headers)) {
			// Length and encoding describe the bytes axios already decoded; forwarding them would
			// describe a body that no longer exists in that shape.
			if (typeof value === "string" && key !== "content-length" && key !== "content-encoding") {
				responseHeaders.set(key, value);
			}
		}

		// 204/205/304 must not carry a body. Axios hands back an empty ArrayBuffer rather than null,
		// and the Response constructor rejects that outright - which used to turn a call that had
		// already succeeded on the API into a 502 for the browser.
		const forbidsBody = [204, 205, 304].includes(response.status);

		return new Response(forbidsBody ? null : response.data, {
			status: response.status,
			headers: responseHeaders,
		});
	} catch (error) {
		if (axios.isAxiosError(error) && error.response) {
			return new Response(error.response.data, {
				status: error.response.status,
				headers: {
					"content-type": (error.response.headers["content-type"] as string) ?? "application/json",
				},
			});
		}

		return new Response("Proxy error", { status: 502 });
	}
}

function forward(
	targetUrl: string,
	method: string,
	headers: Record<string, string>,
	body: ArrayBuffer | undefined,
	auth: { token: string | undefined },
) {
	return axios.request({
		url: targetUrl,
		method,
		paramsSerializer: {
			indexes: null,
		},
		headers: auth.token ? { ...headers, Authorization: `Bearer ${auth.token}` } : headers,
		data: body,
		responseType: "arraybuffer",
		validateStatus: () => true,
	});
}
