import { createServerFn } from "@tanstack/react-start";
import { deleteCookie, getCookie, setCookie } from "@tanstack/react-start/server";
import axios from "axios";
import type { AppUserAuthenticated, OwnerAuthenticated } from "#/api/models";

export const storeToken = createServerFn({ method: "POST" })
	.validator((data: { token: string }) => data)
	.handler(async ({ data }) => {
		setCookie("access_token", data?.token ?? "");
		return { message: "OK" };
	});

export const getToken = createServerFn({
	method: "GET",
}).handler(() => {
	const token = getCookie("access_token");
	return { token };
});

export const hasToken = createServerFn({
	method: "GET",
}).handler(() => {
	const token = getCookie("access_token");
	return { hasToken: token?.length };
});

export const logout = createServerFn({
	method: "POST",
}).handler(async () => {
	deleteCookie("access_token");

	return { success: true };
});

export const getUserAuth = createServerFn({
	method: "GET",
}).handler(async () => {
	const token = getCookie("access_token");

	if (!token) {
		return null;
	}

	const response = await axios.get(`${import.meta.env.VITE_API_URL}/api/user/me`, {
		headers: {
			Authorization: `Bearer ${token}`,
		},
	});

	return response.data as AppUserAuthenticated;
});

export const getOwnerAuth = createServerFn({
	method: "GET",
}).handler(async () => {
	const token = getCookie("access_token");

	if (!token) {
		return null;
	}

	const response = await axios.get(`${import.meta.env.VITE_API_URL}/api/owner/me`, {
		headers: {
			Authorization: `Bearer ${token}`,
		},
	});

	return response.data as OwnerAuthenticated;
});
