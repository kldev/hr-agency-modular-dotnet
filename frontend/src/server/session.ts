import { deleteCookie, getCookie, setCookie } from "@tanstack/react-start/server";
import axios from "axios";
import type { LoginUserResult } from "#/api/models";
import { API_URL } from "#/server/apiUrl";

const ACCESS_COOKIE = "access_token";
const REFRESH_COOKIE = "refresh_token";
const EXPIRES_COOKIE = "access_token_expires_at";

/// Renew a little before the access token actually dies, so a call that starts now cannot arrive
/// after it expired.
const RENEW_MARGIN_MS = 60_000;

/// One exchange per refresh token, shared by everyone who asked for it. Keyed by the token itself,
/// so a token that was already rotated away never shares a slot with its replacement.
const exchanges = new Map<string, Promise<LoginUserResult | undefined>>();

export function storeSession(session: LoginUserResult) {
	// All three live as long as the session does: the access token in the cookie may be stale, but
	// losing the cookie early would only cost a needless round trip to find that out.
	const expires = new Date(session.refreshTokenExpiresAt);

	setCookie(ACCESS_COOKIE, session.token, cookieOptions(expires));
	setCookie(REFRESH_COOKIE, session.refreshToken, cookieOptions(expires));
	setCookie(EXPIRES_COOKIE, session.expiresAt, cookieOptions(expires));
}

/**
 * A session that is one token and nothing else. Clearing the other two cookies is the load-bearing
 * part: all sign-ins share the access cookie, so a refresh token left over from a previous one
 * would quietly renew this session into that one.
 */
function storeAccessTokenOnlySession(token: string) {
	deleteCookie(REFRESH_COOKIE);
	deleteCookie(EXPIRES_COOKIE);

	setCookie(ACCESS_COOKIE, token, {
		httpOnly: true,
		sameSite: "lax",
		secure: process.env.NODE_ENV === "production",
		path: "/",
	});
}

/**
 * The platform owner has no refresh token - that panel signs in again when its access token runs
 * out - so only the bearer is stored.
 */
export function storeOwnerSession(token: string) {
	storeAccessTokenOnlySession(token);
}

/**
 * Signing in as somebody else. The same plumbing as an owner session and deliberately a different
 * name: these are two unrelated things that happen to need one token and no way to renew it, and a
 * call site reading `storeOwnerSession` should never mean "act as this user".
 *
 * Nothing else has to change for the session to work. `currentAccessToken` finds no expiry cookie,
 * falls into `refreshSession`, which finds no refresh token and returns without a request, so the
 * token is used as issued until the API itself refuses it - and then the ordinary 401 handling
 * sends the browser back to the login page.
 */
export function storeImpersonatedSession(token: string) {
	storeAccessTokenOnlySession(token);
}

export function clearSession() {
	for (const name of [ACCESS_COOKIE, REFRESH_COOKIE, EXPIRES_COOKIE]) {
		deleteCookie(name);
	}
}

export function readRefreshToken() {
	return getCookie(REFRESH_COOKIE);
}

/**
 * The access token an outgoing call should carry. Renews first when the stored one is gone or about
 * to expire, so a request does not have to fail before the session is extended.
 */
export async function currentAccessToken(): Promise<string | undefined> {
	const token = getCookie(ACCESS_COOKIE);
	const expiresAt = getCookie(EXPIRES_COOKIE);

	if (token && expiresAt && Date.parse(expiresAt) - Date.now() > RENEW_MARGIN_MS) {
		return token;
	}

	return (await refreshSession()) ?? token;
}

/**
 * Exchanges the refresh token for a new pair. An API that refuses is the API saying the session is
 * over, so the cookies go with it.
 */
export async function refreshSession(): Promise<string | undefined> {
	const refreshToken = getCookie(REFRESH_COOKIE);

	if (!refreshToken) {
		return undefined;
	}

	const session = await exchangeOnce(refreshToken);

	if (!session) {
		clearSession();
		return undefined;
	}

	// Written here rather than inside the exchange: a shared exchange runs in the request context of
	// whoever started it, and every caller has to set the new cookies on its own response.
	storeSession(session);

	return session.token;
}

function exchangeOnce(refreshToken: string) {
	const started = exchanges.get(refreshToken);

	if (started) {
		return started;
	}

	// A page loading several server calls at once will find the same stale token in all of them.
	// Rotation invalidates the token it consumes, so letting each one exchange it would spend the
	// session on the first and make every other look like a replay - which revokes the session.
	const exchange = requestNewSession(refreshToken).finally(() => exchanges.delete(refreshToken));

	exchanges.set(refreshToken, exchange);

	return exchange;
}

async function requestNewSession(refreshToken: string) {
	try {
		const response = await axios.post<LoginUserResult>(`${API_URL}/api/auth/refresh`, {
			refreshToken,
		});

		return response.data;
	} catch {
		return undefined;
	}
}

function cookieOptions(expires: Date) {
	return {
		httpOnly: true,
		sameSite: "lax" as const,
		secure: process.env.NODE_ENV === "production",
		path: "/",
		expires,
	};
}
