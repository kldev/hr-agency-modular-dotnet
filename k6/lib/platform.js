// Shared setup for the scripts that act as agency users: a platform owner, ORG_COUNT organizations
// and an Admin + a Recruiter in each. Runs in setup(), outside the measured part.
//
// The script works on an empty database as well as on a seeded one: it signs the owner in and runs
// /api/development/seed only when there is no owner yet (the seed exists in Development/docker).
// Organizations get slugs from RUN_ID (default: the current time plus a random tail), so a second run on the same
// database creates new ones instead of stopping on a taken slug.

import http from "k6/http";
import { fail, sleep } from "k6";

export const BASE_URL = __ENV.BASE_URL || "http://localhost:5000";
export const ORG_COUNT = Number(__ENV.ORG_COUNT || 5);
// Time plus a random tail: two scripts started in the same millisecond must not share slugs.
export const RUN_ID = __ENV.RUN_ID || `${Date.now()}${Math.floor(Math.random() * 1000)}`;

// The owner of a seeded platform (PlatformSeeder OwnerScenario + Config.TestPassword). A stack
// started with HR_AGENCY_EMAIL/HR_AGENCY_PASSWORD has that account instead - pass its password.
const OWNER_EMAIL = __ENV.OWNER_EMAIL || "admin@hr-agency.com";
const OWNER_PASSWORD = __ENV.OWNER_PASSWORD || "agent999!";

export const USER_PASSWORD = "perf-test-1234";

export const JSON_HEADERS = { "Content-Type": "application/json" };

export function authHeaders(token) {
	return { ...JSON_HEADERS, Authorization: `Bearer ${token}` };
}

const setupTag = { endpoint: "setup" };

function orgSlug(i) {
	return `perf-${RUN_ID}-${i}`;
}

function orgDomain(i) {
	return `${orgSlug(i)}.perf.test`;
}

function loginOwner() {
	const res = http.post(
		`${BASE_URL}/api/owner/login`,
		JSON.stringify({ email: OWNER_EMAIL, password: OWNER_PASSWORD }),
		{
			headers: JSON_HEADERS,
			tags: setupTag,
			// no owner yet is an answer here, not a failed request
			responseCallback: http.expectedStatuses(200, 400, 401, 404),
		},
	);
	return res.status === 200 ? res.json("token") : null;
}

function ensureOwner() {
	let token = loginOwner();
	if (token) return token;

	console.log("No platform owner to sign in as - running /api/development/seed (takes a while)");
	const seed = http.get(`${BASE_URL}/api/development/seed`, { timeout: "10m", tags: setupTag });
	if (seed.status !== 200) fail(`seed failed: status=${seed.status}, body=${seed.body}`);

	token = loginOwner();
	if (!token) {
		fail(
			`cannot sign in as ${OWNER_EMAIL} - if the API created it from HR_AGENCY_PASSWORD, ` +
				"pass that password: -e OWNER_PASSWORD=...",
		);
	}
	return token;
}

function createOrganization(ownerToken, i) {
	const res = http.post(
		`${BASE_URL}/api/organization`,
		JSON.stringify({
			name: `Performance agency ${RUN_ID}-${i}`,
			slug: orgSlug(i),
			emailDomains: [orgDomain(i)],
			info: null,
		}),
		{ headers: authHeaders(ownerToken), tags: setupTag },
	);
	if (res.status !== 201) {
		fail(`organization ${orgSlug(i)} failed: status=${res.status}, body=${res.body}`);
	}
	return res.json("organizationId");
}

function createUser(ownerToken, organizationId, i, role) {
	const email = `${role.toLowerCase()}@${orgDomain(i)}`;
	const body = JSON.stringify({
		email,
		firstName: role,
		lastName: `Perf${i}`,
		role,
		organizationId,
		password: USER_PASSWORD,
	});

	// The organization was created a moment ago; give its read side a few tries to catch up.
	for (let attempt = 0; attempt < 20; attempt++) {
		const res = http.post(`${BASE_URL}/api/organization/users`, body, {
			headers: authHeaders(ownerToken),
			tags: setupTag,
			responseCallback: http.expectedStatuses(201, 400, 404),
		});
		if (res.status === 201) return { email, slug: orgSlug(i) };
		sleep(0.5);
	}
	fail(`user ${email} could not be created`);
}

// Signing in reads UserProjection, which the async daemon fills - until it does, login fails.
// Waiting here keeps projection lag out of the measured numbers.
function waitUntilCanLogin(user) {
	const body = JSON.stringify({ email: user.email, password: USER_PASSWORD, slug: user.slug });
	for (let attempt = 0; attempt < 60; attempt++) {
		const res = http.post(`${BASE_URL}/api/auth/login`, body, {
			headers: JSON_HEADERS,
			tags: setupTag,
			responseCallback: http.expectedStatuses(200, 400, 401, 404, 500),
		});
		if (res.status === 200) return;
		sleep(1);
	}
	fail(`user ${user.email} still cannot sign in - is the projection daemon running?`);
}

/** Owner, organizations and users; returns what the VUs need. */
export function preparePlatform() {
	const ownerToken = ensureOwner();
	const users = [];

	for (let i = 1; i <= ORG_COUNT; i++) {
		const organizationId = createOrganization(ownerToken, i);
		users.push(createUser(ownerToken, organizationId, i, "Admin"));
		users.push(createUser(ownerToken, organizationId, i, "Recruiter"));
	}

	for (const user of users) waitUntilCanLogin(user);

	console.log(`Created ${ORG_COUNT} organizations and ${users.length} users (run ${RUN_ID})`);
	return { users };
}

/** Signs a user in; null (and a failed check upstream) when that does not work. */
export function login(user, tags = { endpoint: "login" }) {
	const res = http.post(
		`${BASE_URL}/api/auth/login`,
		JSON.stringify({ email: user.email, password: USER_PASSWORD, slug: user.slug }),
		{ headers: JSON_HEADERS, tags },
	);
	if (res.status !== 200) {
		console.error(`LOGIN ERROR: user=${user.email}, status=${res.status}, body=${res.body}`);
		return null;
	}
	return res.json("token");
}
