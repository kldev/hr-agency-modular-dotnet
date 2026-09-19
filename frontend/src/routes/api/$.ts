import { createFileRoute } from "@tanstack/react-router";
import { getCookie } from "@tanstack/react-start/server";
import axios from "axios";

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
export const API_URL = process.env.API_URL ?? "http://localhost:5000";

async function proxyRequest(request: Request) {
	const url = new URL(request.url);
	const token = getCookie("access_token");

	const targetUrl = `${API_URL}/api${url.pathname.replace(/^\/api/, "")}${url.search}`;

	const headers: Record<string, string> = {};

	request.headers.forEach((value, key) => {
		if (key !== "host" && key !== "content-length") {
			headers[key] = value;
		}
	});

	if (token) {
		headers.Authorization = `Bearer ${token}`;
	}

	try {
		const response = await axios.request({
			url: targetUrl,
			method: request.method,
			paramsSerializer: {
				indexes: null,
			},
			headers,
			data:
				request.method === "GET" || request.method === "HEAD"
					? undefined
					: await request.arrayBuffer(),
			responseType: "arraybuffer",
			validateStatus: () => true,
		});

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
