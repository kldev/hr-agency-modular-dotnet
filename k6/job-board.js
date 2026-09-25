// Candidates browsing the public job board (HrAgencySystem.Web, :5050): the list page, the feed it
// reads, and an offer page. Anonymous - the board talks to the API with its own service key, so
// this exercises web -> API internal routes -> feed file in RustFS. Grafana: "HR Agency overview"
// with hr-web and hr-api selected. Needs published posts and a generated feed: seed the platform
// (/api/development/seed) and keep feeds-worker running.
//
//   k6 run k6/job-board.js
//   k6 run -e SLUGS=hr-agency,flex-jobs -e VUS=30 -e DURATION=5m k6/job-board.js

import http from "k6/http";
import { check, fail, sleep } from "k6";
import { randomIntBetween, randomItem } from "https://jslib.k6.io/k6-utils/1.4.0/index.js";

const BASE_URL = __ENV.BASE_URL || "http://localhost:5000";
const WEB_URL = __ENV.WEB_URL || "http://localhost:5050";
const SLUGS = (__ENV.SLUGS || "hr-agency,flex-jobs,tech-jobs").split(",").map((s) => s.trim());

export const options = {
	scenarios: {
		candidates: {
			executor: "constant-vus",
			vus: Number(__ENV.VUS || 10),
			duration: __ENV.DURATION || "3m",
		},
	},
	thresholds: {
		http_req_failed: ["rate<0.01"],
		"http_req_duration{endpoint:board-list}": ["p(95)<800"],
		"http_req_duration{endpoint:board-offer}": ["p(95)<800"],
		"http_req_duration{endpoint:feed-json}": ["p(95)<500"],
	},
};

// The offer pages, read once from each agency's feed: the apply url is where a candidate lands.
export function setup() {
	const offers = [];
	for (const slug of SLUGS) {
		const res = http.get(`${BASE_URL}/p/${slug}/jobs.json`, {
			tags: { endpoint: "setup" },
			responseCallback: http.expectedStatuses(200, 404),
		});
		if (res.status !== 200) {
			console.warn(`no feed for ${slug} yet (status ${res.status}) - skipped`);
			continue;
		}
		for (const job of res.json("jobs") || []) {
			// The feed carries the board's public address; point it at WEB_URL.
			const path = job.applyUrl.replace(/^https?:\/\/[^/]+/, "");
			offers.push({ slug, path });
		}
	}
	if (offers.length === 0) fail("no offers in any feed - seed the platform and run feeds-worker");
	console.log(`${offers.length} offers from ${SLUGS.length} boards`);
	return { offers };
}

export default function (data) {
	const offer = randomItem(data.offers);

	const list = http.get(`${WEB_URL}/${offer.slug}/jobs.html`, { tags: { endpoint: "board-list" } });
	check(list, { "board list is 200": (r) => r.status === 200 });

	// What the list page's script fetches.
	const feed = http.get(`${WEB_URL}/${offer.slug}/jobs.json`, { tags: { endpoint: "feed-json" } });
	check(feed, { "feed is 200": (r) => r.status === 200 });
	sleep(randomIntBetween(300, 1500) / 1000);

	const page = http.get(`${WEB_URL}${offer.path}`, { tags: { endpoint: "board-offer" } });
	check(page, { "offer page is 200": (r) => r.status === 200 });
	sleep(randomIntBetween(500, 2000) / 1000);
}
