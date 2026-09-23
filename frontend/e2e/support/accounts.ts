/*
 * Seeded demo accounts (`PlatformSeeder/Config`), not credentials of anybody real: every seeded
 * user shares one password, and the same values prefill the dev login form from `.env`.
 * Override with E2E_EMAIL / E2E_PASSWORD to run against another tenant.
 */
export const demoUser = {
	email: process.env.E2E_EMAIL ?? "j.smith@hr-agency.com",
	password: process.env.E2E_PASSWORD ?? "agent999!",
};
