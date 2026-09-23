import { expect, test } from "@playwright/test";
import { reloadUntilVisible } from "../support/api";
import { open } from "../support/navigation";
import { docShot } from "../support/screenshots";
import { chooseSuggestion, pickSuggestion } from "../support/ui";

/*
 * A recruitment team built from people the seed leaves outside any team (a person belongs to at
 * most one): created with its first members, then grown from its own page.
 */
const teamName = "Key Accounts";

const founders = [
	{ query: "Krzysztof", name: /Krzysztof Baran/, role: "Lead" },
	{ query: "Agnieszka", name: /Agnieszka Mazur/, role: "Recruiter" },
	{ query: "Rafal", name: /Rafal Wojcik/, role: "Operations" },
] as const;

test("creates a team and adds a member", async ({ page }) => {
	await open(page, "/app/teams");
	await page.getByRole("button", { name: "Add team" }).click();

	const drawer = page.getByRole("dialog", { name: "Create team" });
	await drawer.getByLabel("Name").fill(teamName);

	for (const [index, member] of founders.entries()) {
		if (index > 0) {
			await drawer.getByRole("button", { name: "Add member" }).click();
		}

		await chooseSuggestion(
			drawer.getByPlaceholder(`Person ${index + 1}`),
			member.query,
			member.name,
		);
		await drawer.getByLabel(`Team role ${index + 1}`).selectOption(member.role);
	}

	await drawer.getByRole("button", { name: "Save changes" }).click();
	await expect(drawer).toBeHidden();

	const row = page.getByRole("row").filter({ hasText: teamName }).first();
	await reloadUntilVisible(page, () => expect(row).toBeVisible({ timeout: 2_000 }));
	// Rows open on a double click (`MainTable`); a single click only selects.
	await row.getByText(teamName, { exact: true }).dblclick();

	await expect(page).toHaveURL(/\/app\/teams\/[0-9a-f-]{36}/);
	await expect(page.getByRole("heading", { name: teamName })).toBeVisible();

	await page.getByRole("button", { name: "Add member" }).click();
	const add = page.getByRole("dialog", { name: `Add to ${teamName}` });
	await pickSuggestion(add, "Person", "Iryna", /Iryna Kovalenko/);
	await add.getByLabel("Team role").selectOption("Recruiter");
	await add.getByRole("button", { name: "Save changes" }).click();
	await expect(add).toBeHidden();

	for (const name of ["Krzysztof Baran", "Agnieszka Mazur", "Rafal Wojcik", "Iryna Kovalenko"]) {
		await expect(page.getByText(name).first()).toBeVisible();
	}

	await docShot(page, "teams");
});
