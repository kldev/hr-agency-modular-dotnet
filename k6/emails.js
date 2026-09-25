// Mail traffic: password reset requests for the perf users. Each one starts a PasswordResetSaga that
// publishes SendPasswordReset to x.emails -> q.emails.identity -> NotificationWorker -> SMTP
// (Mailpit, http://localhost:8025). Grafana: "Emails worker" and "RabbitMQ".
//
//   k6 run k6/emails.js
//   k6 run -e RATE=10 -e DURATION=5m k6/emails.js

import http from "k6/http";
import { check } from "k6";
import { randomItem } from "https://jslib.k6.io/k6-utils/1.4.0/index.js";
import { BASE_URL, JSON_HEADERS, preparePlatform } from "./lib/platform.js";

export const options = {
	setupTimeout: "15m",
	scenarios: {
		password_resets: {
			executor: "constant-arrival-rate",
			rate: Number(__ENV.RATE || 3),
			timeUnit: "1s",
			duration: __ENV.DURATION || "3m",
			preAllocatedVUs: 5,
			maxVUs: 20,
		},
	},
	thresholds: {
		http_req_failed: ["rate<0.01"],
		"http_req_duration{endpoint:password-reset}": ["p(95)<500"],
	},
};

export function setup() {
	return preparePlatform();
}

export default function (data) {
	const user = randomItem(data.users);
	const res = http.post(
		`${BASE_URL}/api/auth/password-reset`,
		JSON.stringify({ email: user.email, slug: user.slug }),
		{ headers: JSON_HEADERS, tags: { endpoint: "password-reset" } },
	);
	check(res, { "password reset accepted (202)": (r) => r.status === 202 });
}
