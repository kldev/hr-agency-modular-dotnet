import { defineConfig, devices } from "@playwright/test";

/*
 * End-to-end tests drive the real panel against the real API, so the stack has to be up and
 * seeded (`GET /api/development/seed`) before a run - see e2e/README section in the root Readme.
 * The seed is not repeatable, so a second run starts from a reset database and a fresh seed.
 */
const baseURL = process.env.E2E_BASE_URL ?? "http://localhost:4300";

export const authFile = "playwright/.auth/user.json";

/** A 2K desktop - the size the documentation screenshots are taken at. */
const viewport = { width: 2560, height: 1440 };

export default defineConfig({
	testDir: "./e2e",
	outputDir: "./test-results",
	// One worker: the tests share one seeded tenant and every write waits on async projections.
	fullyParallel: false,
	workers: 1,
	retries: 0,
	forbidOnly: !!process.env.CI,
	timeout: 90_000,
	expect: { timeout: 15_000 },
	reporter: [["list"], ["html", { open: "never" }]],
	use: {
		baseURL,
		viewport,
		colorScheme: "light",
		locale: "en-GB",
		timezoneId: "Europe/Warsaw",
		actionTimeout: 15_000,
		trace: "retain-on-failure",
		screenshot: "only-on-failure",
	},
	projects: [
		{ name: "setup", testMatch: /auth\.setup\.ts/ },
		{
			name: "chromium",
			use: {
				...devices["Desktop Chrome"],
				viewport,
				storageState: authFile,
			},
			dependencies: ["setup"],
			testIgnore: /auth\.setup\.ts/,
		},
	],
	webServer: {
		command: "yarn dev",
		url: `${baseURL}/login`,
		reuseExistingServer: true,
		timeout: 120_000,
	},
});
