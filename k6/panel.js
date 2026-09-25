// Load on the agency panel API: a recruiter signs in, looks at applications, browses candidates
// (no filter / search / source) and adds one. Grafana: "HR Agency overview" (hr-api) and PostgreSQL.
//
//   k6 run k6/panel.js
//   k6 run -e VUS=50 -e DURATION=5m -e ORG_COUNT=10 k6/panel.js

import http from "k6/http";
import { check, sleep } from "k6";
import { randomIntBetween, randomItem } from "https://jslib.k6.io/k6-utils/1.4.0/index.js";
import { BASE_URL, authHeaders, login, preparePlatform } from "./lib/platform.js";

export const options = {
	setupTimeout: "15m",
	scenarios: {
		recruiter_session: {
			executor: "constant-vus",
			vus: Number(__ENV.VUS || 20),
			duration: __ENV.DURATION || "3m",
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

export function setup() {
	return preparePlatform();
}

function get(token, url, endpoint) {
	const res = http.get(url, { headers: authHeaders(token), tags: { endpoint } });
	const ok = check(res, { [`${endpoint} status is 200`]: (r) => r.status === 200 });
	if (!ok) console.error(`${endpoint} ERROR: status=${res.status}, body=${res.body}`);
	return res;
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
	if (!check(token, { "login returned a token": (t) => Boolean(t) })) return;

	get(token, `${BASE_URL}/api/recruitment/job-applications?page=1&pageSize=20`, "job-applications");
	sleep(randomIntBetween(100, 300) / 1000);

	readCandidates(token);
	createCandidate(token);

	sleep(randomIntBetween(200, 700) / 1000);
}
