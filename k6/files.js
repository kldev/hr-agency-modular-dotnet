// Upload traffic through the file service: users replace their avatar (API -> file service ->
// RustFS bucket "documents"). About one upload in ten is named .txt while claiming to be a PNG; the
// API lets it through (the content type is fine) and the file service refuses it as an extension
// mismatch - which is what the "rejected" panels are for. Grafana: "File service".
//
//   k6 run k6/files.js
//   k6 run -e VUS=10 -e DURATION=5m -e REJECT_RATE=0.2 k6/files.js

import http from "k6/http";
import encoding from "k6/encoding";
import { check, sleep } from "k6";
import { randomItem } from "https://jslib.k6.io/k6-utils/1.4.0/index.js";
import { BASE_URL, login, preparePlatform } from "./lib/platform.js";

const REJECT_RATE = Number(__ENV.REJECT_RATE || 0.1);

// A valid 1x1 transparent PNG.
const PNG = encoding.b64decode(
	"iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==",
);

export const options = {
	setupTimeout: "15m",
	scenarios: {
		avatars: {
			executor: "constant-vus",
			vus: Number(__ENV.VUS || 5),
			duration: __ENV.DURATION || "3m",
		},
	},
	thresholds: {
		checks: ["rate>0.99"],
		"http_req_duration{endpoint:avatar-upload}": ["p(95)<1000"],
	},
};

export function setup() {
	return preparePlatform();
}

export default function (data) {
	const token = login(randomItem(data.users));
	if (!check(token, { "login returned a token": (t) => Boolean(t) })) return;

	const rejected = Math.random() < REJECT_RATE;
	const fileName = rejected ? "avatar.txt" : "avatar.png";

	const res = http.post(
		`${BASE_URL}/api/users/me/avatar`,
		{ file: http.file(PNG, fileName, "image/png") },
		{
			headers: { Authorization: `Bearer ${token}` },
			tags: { endpoint: rejected ? "avatar-upload-rejected" : "avatar-upload" },
			// a refused upload is the expected answer for the mismatched name
			responseCallback: rejected ? http.expectedStatuses(400) : http.expectedStatuses(200),
		},
	);

	if (rejected) {
		check(res, { "mismatched upload refused (400)": (r) => r.status === 400 });
	} else {
		check(res, { "avatar stored (200)": (r) => r.status === 200 });
	}

	sleep(0.5);
}
