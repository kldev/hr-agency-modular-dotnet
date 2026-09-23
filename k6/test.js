// Load test of the agency panel API.
//
// UWAGA: skrypt zakłada PUSTĄ bazę (świeży `docker compose up`, bez wcześniejszego seeda i bez
// poprzednich przebiegów). setup() sam woła /api/development/seed, żeby powstał właściciel
// platformy, a potem zakłada organizacje perf-org-1..N ze stałymi slugami i domenami - drugi
// przebieg na tej samej bazie trafi na zajęte slugi (400) i przerwie setup. Na bazie, która nie jest
// pusta, podaj inny RUN_ID (np. RUN_ID=$(date +%s)), wtedy slugi i domeny będą nowe.
//
//   k6 run k6/test.js
//   k6 run -e BASE_URL=http://localhost:5000 -e ORG_COUNT=20 -e VUS=50 -e DURATION=2m k6/test.js

import http from "k6/http";
import { check, fail, sleep } from "k6";
import { randomIntBetween, randomItem } from "https://jslib.k6.io/k6-utils/1.4.0/index.js";

const BASE_URL = __ENV.BASE_URL || "http://localhost:5000";
const ORG_COUNT = Number(__ENV.ORG_COUNT || 10);
const RUN_ID = __ENV.RUN_ID || "";

// The seeded platform owner - PlatformSeeder/Scenario/OwnerScenario + Config.TestPassword.
const OWNER_EMAIL = __ENV.OWNER_EMAIL || "admin@hr-agency.com";
const OWNER_PASSWORD = __ENV.OWNER_PASSWORD || "agent999!";

const USER_PASSWORD = "perf-test-1234";

export const options = {
	setupTimeout: "10m",
	scenarios: {
		recruiter_session: {
			executor: "constant-vus",
			vus: Number(__ENV.VUS || 30),
			duration: __ENV.DURATION || "1m",
		},
	},
	thresholds: {
		http_req_failed: ["rate<0.01"],
		"http_req_duration{endpoint:login}": ["p(95)<500"],
		"http_req_duration{endpoint:job-applications}": ["p(95)<500"],
		"http_req_duration{endpoint:candidates-no-filter}": ["p(95)<500"],
		"http_req_duration{endpoint:candidates-search}": ["p(95)<500"],
		"http_req_duration{endpoint:candidates-source}": ["p(95)<500"],
		"http_req_duration{endpoint:candidate-create}": ["p(95)<800"],
	},
};

const CANDIDATE_SOURCES = [
	"CareerPage",
	"PracujPl",
	"Olx",
	"PracaPl",
	"RocketJobs",
	"JustJoinIt",
	"NoFluffJobs",
	"LinkedIn",
	"Indeed",
	"Referral",
	"DirectSourcing",
	"InternalDatabase",
	"RecruitmentAgency",
	"DirectApplication",
	"Facebook",
	"Other",
	"Direct",
];

const SEARCH_VALUES = ["Load", "Candidate", "net"];

const JSON_HEADERS = { "Content-Type": "application/json" };

function authHeaders(token) {
	return { ...JSON_HEADERS, Authorization: `Bearer ${token}` };
}

function orgSlug(i) {
	return RUN_ID ? `perf-${RUN_ID}-org-${i}` : `perf-org-${i}`;
}

function orgDomain(i) {
	return `${orgSlug(i)}.perf.test`;
}

// ---------------------------------------------------------------------------------------------
// setup: owner -> organizations -> one admin and one recruiter per organization
// ---------------------------------------------------------------------------------------------

function loginOwner() {
	const res = http.post(
		`${BASE_URL}/api/owner/login`,
		JSON.stringify({ email: OWNER_EMAIL, password: OWNER_PASSWORD }),
		{ headers: JSON_HEADERS, tags: { endpoint: "setup" } },
	);
	return res.status === 200 ? res.json("token") : null;
}

function ensureOwner() {
	let token = loginOwner();
	if (token) return token;

	// Empty database: there is no owner yet, and the only way to get one is the dev seed.
	console.log("No platform owner yet - running /api/development/seed (may take a while)");
	const seed = http.get(`${BASE_URL}/api/development/seed`, {
		timeout: "5m",
		tags: { endpoint: "setup" },
	});
	if (seed.status !== 200) fail(`seed failed: status=${seed.status}, body=${seed.body}`);

	token = loginOwner();
	if (!token) fail("owner login failed after seeding");
	return token;
}

function createOrganization(ownerToken, i) {
	const res = http.post(
		`${BASE_URL}/api/organization`,
		JSON.stringify({
			name: `Performance agency ${i}`,
			slug: orgSlug(i),
			emailDomains: [orgDomain(i)],
			info: null,
		}),
		{ headers: authHeaders(ownerToken), tags: { endpoint: "setup" } },
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
	for (let attempt = 0; attempt < 10; attempt++) {
		const res = http.post(`${BASE_URL}/api/organization/users`, body, {
			headers: authHeaders(ownerToken),
			tags: { endpoint: "setup" },
		});
		if (res.status === 201) return { email, slug: orgSlug(i) };
		if (attempt === 9) fail(`user ${email} failed: status=${res.status}, body=${res.body}`);
		sleep(0.5);
	}
}

// Signing in reads UserProjection, which the async daemon fills - until it does, login answers 500.
// Waiting here keeps projection lag out of the measured login numbers.
function waitUntilCanLogin(user) {
	const body = JSON.stringify({ email: user.email, password: USER_PASSWORD, slug: user.slug });
	for (let attempt = 0; attempt < 60; attempt++) {
		const res = http.post(`${BASE_URL}/api/auth/login`, body, {
			headers: JSON_HEADERS,
			tags: { endpoint: "setup" },
			// a 500 while the projection catches up is expected, not a failed request
			responseCallback: http.expectedStatuses(200, 500),
		});
		if (res.status === 200) return;
		sleep(1);
	}
	fail(`user ${user.email} still cannot sign in - is the projection daemon running?`);
}

export function setup() {
	const ownerToken = ensureOwner();
	const users = [];

	for (let i = 1; i <= ORG_COUNT; i++) {
		const organizationId = createOrganization(ownerToken, i);
		users.push(createUser(ownerToken, organizationId, i, "Admin"));
		users.push(createUser(ownerToken, organizationId, i, "Recruiter"));
	}

	for (const user of users) waitUntilCanLogin(user);

	console.log(`Created ${ORG_COUNT} organizations and ${users.length} users`);
	return { users };
}

// ---------------------------------------------------------------------------------------------
// VU: a recruiter's session - sign in, look at applications, browse candidates, add one
// ---------------------------------------------------------------------------------------------

function login(user) {
	const res = http.post(
		`${BASE_URL}/api/auth/login`,
		JSON.stringify({ email: user.email, password: USER_PASSWORD, slug: user.slug }),
		{ headers: JSON_HEADERS, tags: { endpoint: "login" } },
	);

	const ok = check(res, {
		"login status is 200": (r) => r.status === 200,
		"login contains token": (r) => r.status === 200 && Boolean(r.json("token")),
	});
	if (!ok) {
		console.error(`LOGIN ERROR: user=${user.email}, status=${res.status}, body=${res.body}`);
		return null;
	}
	return res.json("token");
}

function get(token, url, endpoint) {
	const res = http.get(url, { headers: authHeaders(token), tags: { endpoint } });
	const ok = check(res, { [`${endpoint} status is 200`]: (r) => r.status === 200 });
	if (!ok) console.error(`${endpoint} ERROR: status=${res.status}, body=${res.body}`);
	return res;
}

function readJobApplications(token) {
	return get(
		token,
		`${BASE_URL}/api/recruitment/job-applications?page=1&pageSize=20`,
		"job-applications",
	);
}

// 40% no filter, 35% search, 25% source filter
function readCandidates(token) {
	const p = Math.random();
	let query = "";
	let endpoint = "candidates-no-filter";

	if (p >= 0.4 && p < 0.75) {
		query = `&search=${encodeURIComponent(randomItem(SEARCH_VALUES))}`;
		endpoint = "candidates-search";
	} else if (p >= 0.75) {
		query = `&source=${randomItem(CANDIDATE_SOURCES)}`;
		endpoint = "candidates-source";
	}

	return get(token, `${BASE_URL}/api/recruitment/candidates?page=1&pageSize=20${query}`, endpoint);
}

function createCandidate(token) {
	const res = http.post(
		`${BASE_URL}/api/recruitment/candidates`,
		JSON.stringify({
			email: `candidate-${__VU}-${__ITER}-${Date.now()}@load-test.local`,
			phoneNumber: `+48123${String(__VU).padStart(2, "0")}${String(__ITER % 10000).padStart(4, "0")}`,
			firstName: `LoadTest${__VU}`,
			lastName: `Candidate${__ITER}`,
			source: randomItem(CANDIDATE_SOURCES),
			note: "",
		}),
		{ headers: authHeaders(token), tags: { endpoint: "candidate-create" } },
	);

	const ok = check(res, { "candidate create status is 201": (r) => r.status === 201 });
	if (!ok) console.error(`CANDIDATE CREATE ERROR: status=${res.status}, body=${res.body}`);
	return res;
}

export default function (data) {
	const token = login(randomItem(data.users));
	if (!token) return;

	readJobApplications(token);
	sleep(randomIntBetween(100, 300) / 1000);

	readCandidates(token);
	createCandidate(token);

	sleep(randomIntBetween(200, 700) / 1000);
}
