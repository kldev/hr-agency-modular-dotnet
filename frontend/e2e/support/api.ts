import { expect, type Page } from "@playwright/test";

/*
 * Fixtures the flow under test does not create itself go straight through the API. The panel's
 * own `/api/*` proxy is used, so the request carries the signed-in session cookie exactly like the
 * UI does and no token is ever handled here.
 */

type JobPostRow = { id: string; title: string; status: string };

/** A published job post of the seed, found by its title - the only kind that takes applications. */
export async function findPublishedJobPost(page: Page, title: string): Promise<JobPostRow> {
	const response = await page.request.get("/api/recruitment/job-posting", {
		params: { search: title, status: "Published", pageSize: 20 },
	});
	expect(response.ok(), `GET job posts: ${response.status()}`).toBeTruthy();

	const { content } = (await response.json()) as { content: JobPostRow[] };
	const post = content.find((row) => row.title === title && row.status === "Published");

	expect(post, `the seed has no published "${title}" post`).toBeDefined();
	return post as JobPostRow;
}

export type Applicant = {
	firstName: string;
	lastName: string;
	email: string;
	phoneNumber: string;
};

/**
 * Files an application the way a job board does; the candidate is created from the e-mail.
 * Skipped when that person has already applied to the post, so a failed run can be repeated.
 */
export async function applyToJobPost(page: Page, jobPostId: string, applicant: Applicant) {
	const existing = await page.request.get("/api/recruitment/job-applications", {
		params: { search: applicant.email, jobPostId, pageSize: 1 },
	});
	expect(existing.ok(), `GET applications: ${existing.status()}`).toBeTruthy();

	if (((await existing.json()) as { content: unknown[] }).content.length > 0) {
		return;
	}

	const response = await page.request.post(`/api/recruitment/job-posting/${jobPostId}/apply`, {
		data: { ...applicant, source: "JustJoinIt" },
	});
	expect(response.ok(), `apply: ${response.status()} ${await response.text()}`).toBeTruthy();
}

/**
 * Read models are built by Marten's async daemon, so a row written a moment ago may not be on a
 * list yet. Reloads until it is, instead of sleeping for a guessed amount of time.
 */
export async function reloadUntilVisible(page: Page, assertion: () => Promise<void>) {
	await expect(async () => {
		await page.reload();
		await page.waitForLoadState("networkidle");
		await assertion();
	}).toPass({ timeout: 30_000, intervals: [500, 1_000, 2_000] });
}

/**
 * The client of the recruitment and project flows. The seed's companies have generated names, so
 * the demo client is created here - with a complete profile, which a project needs to go live.
 * Looked up by name first, so a repeated run reuses it instead of tripping the tax id reservation.
 */
export const demoClient = {
	name: "ACME Automotive",
	legalName: "ACME Automotive Sp. z o.o.",
	taxId: "5252344078",
};

export async function ensureDemoClient(page: Page): Promise<string> {
	const found = await page.request.get("/api/companies", {
		params: { search: demoClient.name, pageSize: 5 },
	});
	expect(found.ok(), `GET companies: ${found.status()}`).toBeTruthy();

	const existing = (
		(await found.json()) as { content: { id: string; name: string }[] }
	).content.find((company) => company.name === demoClient.name);

	if (existing) {
		return existing.id;
	}

	const created = await page.request.post("/api/companies", {
		data: {
			name: demoClient.name,
			countryCode: "PL",
			taxId: demoClient.taxId,
			registrationNumber: "0000481516",
			website: "https://acme-automotive.example.com",
			industry: "Automotive",
			contact: {
				email: "hr@acme-automotive.example.com",
				firstName: "Anna",
				lastName: "Kowalska",
				jobTitle: "HR Director",
				phone: "+48 77 441 20 00",
			},
		},
	});
	expect(created.ok(), `create company: ${created.status()} ${await created.text()}`).toBeTruthy();
	const { companyId } = (await created.json()) as { companyId: string };

	const profile = await page.request.put(`/api/companies/${companyId}/profile`, {
		data: {
			legalName: demoClient.legalName,
			street: "ul. Fabryczna",
			buildingNumber: "8",
			unitNumber: null,
			postalCode: "45-125",
			city: "Opole",
			countryCode: "PL",
			vatNumber: `PL${demoClient.taxId}`,
			iban: "PL27114020040000300201355387",
			bic: "BREXPLPW",
			legalRepresentative: {
				email: "board@acme-automotive.example.com",
				firstName: "Tomasz",
				lastName: "Wiśniewski",
				jobTitle: "Managing Director",
				phone: "+48 77 441 20 01",
			},
		},
	});
	expect(
		profile.ok(),
		`complete profile: ${profile.status()} ${await profile.text()}`,
	).toBeTruthy();

	return companyId;
}

/**
 * A sales opportunity with a history: created for the demo client, walked through the pipeline to
 * a proposal and given a couple of activities, so its details page reads like a real deal. The
 * random `seed-sales` data fills the board around it but is different on every seed.
 */
export async function createDemoOpportunity(page: Page, companyId: string, title: string) {
	const created = await page.request.post("/api/sales/opportunity", {
		data: {
			companyId,
			title,
			description:
				"ACME wants its own connected-car platform team in Opole: three senior .NET engineers, a frontend developer and a DevOps engineer, starting in October.",
			expectedValue: 96000,
			isHotLead: true,
			currency: "PLN",
			expectedCloseDate: "2026-10-15",
			responsibleId: null,
		},
	});
	expect(
		created.ok(),
		`create opportunity: ${created.status()} ${await created.text()}`,
	).toBeTruthy();
	const { opportunityId } = (await created.json()) as { opportunityId: string };

	for (const stage of ["Viewed", "Contacted", "Qualified", "Proposal"]) {
		const moved = await page.request.put(`/api/sales/opportunity/${opportunityId}/stage`, {
			data: { stage },
		});
		expect(moved.ok(), `stage ${stage}: ${moved.status()} ${await moved.text()}`).toBeTruthy();
	}

	const activities = [
		["Call", "Discovery call with the HR director - team of five, hybrid in Opole."],
		["Meeting", "Workshop on the platform roadmap and the seniority mix of the team."],
		["Email", "Sent the proposal: outsourced team, 12 months, monthly invoicing."],
	] as const;

	for (const [type, note] of activities) {
		const logged = await page.request.post("/api/sales/activity", {
			data: { opportunityId, type, note },
		});
		expect(logged.ok(), `activity: ${logged.status()} ${await logged.text()}`).toBeTruthy();
	}

	return opportunityId;
}

type ApplicationRow = { id: string; status: string };

async function applicationsIn(page: Page, status: string, count: number) {
	const response = await page.request.get("/api/recruitment/job-applications", {
		params: { status, pageSize: count },
	});
	expect(response.ok(), `GET applications: ${response.status()}`).toBeTruthy();

	return ((await response.json()) as { content: ApplicationRow[] }).content;
}

async function changeStatus(page: Page, applicationId: string, status: string) {
	const response = await page.request.put(
		`/api/recruitment/job-applications/${applicationId}/status`,
		{ data: { status, note: null, interviewId: null } },
	);
	expect(
		response.ok(),
		`status ${status}: ${response.status()} ${await response.text()}`,
	).toBeTruthy();
}

/**
 * The seed leaves every application at "Applied" or "Interview", so a funnel of it has nothing
 * past the interview. This walks a handful further the way a recruiter would - offers, hires, an
 * assessment, a few rejections - so the dashboard has a whole funnel to draw.
 */
export async function moveApplicationsThroughFunnel(page: Page) {
	const interviewed = await applicationsIn(page, "Interview", 10);
	const applied = await applicationsIn(page, "Applied", 3);

	for (const [index, application] of interviewed.entries()) {
		if (index < 6) {
			await changeStatus(page, application.id, "Offer");

			if (index < 4) {
				await changeStatus(page, application.id, "Hired");
			}
		} else if (index < 8) {
			await changeStatus(page, application.id, "Assessment");
		}
	}

	for (const application of applied) {
		await changeStatus(page, application.id, "Rejected");
	}
}

/** Waits until the reports service sees the hires - its tables are filled by async projections. */
export async function waitForReportedHires(page: Page, atLeast: number) {
	await expect(async () => {
		const response = await page.request.get("/api/reports/recruitment");
		expect(response.ok()).toBeTruthy();

		const report = (await response.json()) as { totals: { hires: number } };
		expect(report.totals.hires).toBeGreaterThanOrEqual(atLeast);
	}).toPass({ timeout: 30_000, intervals: [500, 1_000, 2_000] });
}
