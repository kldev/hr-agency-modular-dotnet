import { createServerFn } from "@tanstack/react-start";
import axios from "axios";
import type { AppUserAuthenticated, LoginUserResult, OwnerAuthenticated } from "#/api/models";
import { API_URL } from "#/server/apiUrl";
import {
	clearSession,
	currentAccessToken,
	readRefreshToken,
	storeOwnerSession,
	storeSession,
} from "#/server/session";

export const storeToken = createServerFn({ method: "POST" })
	.validator((data: LoginUserResult) => data)
	.handler(async ({ data }) => {
		storeSession(data);
		return { message: "OK" };
	});

export const storeOwnerToken = createServerFn({ method: "POST" })
	.validator((data: { token: string }) => data)
	.handler(async ({ data }) => {
		storeOwnerSession(data.token);
		return { message: "OK" };
	});

export const logout = createServerFn({
	method: "POST",
}).handler(async () => {
	const refreshToken = readRefreshToken();

	// The cookies go regardless: a server that cannot be reached must not leave the user signed in.
	clearSession();

	if (!refreshToken) {
		return { success: true };
	}

	try {
		// Without this the refresh token would stay usable for the rest of its thirty days, and
		// signing out would only mean "this browser forgot".
		await axios.post(`${API_URL}/api/auth/logout`, { refreshToken });
	} catch {
		return { success: false };
	}

	return { success: true };
});

export const getUserAuth = createServerFn({
	method: "GET",
}).handler(async () => {
	return await whoAmI<AppUserAuthenticated>("/api/user/me");
});

export const getOwnerAuth = createServerFn({
	method: "GET",
}).handler(async () => {
	return await whoAmI<OwnerAuthenticated>("/api/owner/me");
});

async function whoAmI<T>(path: string) {
	const token = await currentAccessToken();

	if (!token) {
		return null;
	}

	try {
		const response = await axios.get<T>(`${API_URL}${path}`, {
			headers: { Authorization: `Bearer ${token}` },
		});

		return response.data;
	} catch {
		// The token was refused although it was just renewed, so there is no session left to report.
		return null;
	}
}
