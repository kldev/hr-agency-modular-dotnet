import { expect, test } from "@playwright/test";
import { demoUser } from "../support/accounts";
import { open } from "../support/navigation";
import { docShot } from "../support/screenshots";

// The one test that has to start signed out.
test.use({ storageState: { cookies: [], origins: [] } });

test("signs in and lands in the panel", async ({ page }) => {
	await open(page, "/login");

	await expect(page.getByRole("heading", { name: "Welcome back" })).toBeVisible();

	const email = page.getByLabel("Email address");
	const password = page.getByLabel("Password", { exact: true });
	const signIn = page.getByRole("button", { name: "Sign in" });

	await expect(email).toBeEditable();
	await expect(password).toBeEditable();
	await expect(signIn).toBeEnabled();

	// The dev build prefills the form from .env; typing over it proves the fields are real.
	await email.fill(demoUser.email);
	await password.fill(demoUser.password);

	await docShot(page, "login");

	await signIn.click();

	await expect(page).toHaveURL(/\/app\/dashboard/);
	await expect(page.getByRole("navigation").first()).toBeVisible();
	await expect(page.getByRole("link", { name: "Job postings" })).toBeVisible();
	await expect(page.getByRole("link", { name: "Projects" })).toBeVisible();
});
