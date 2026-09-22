import type { OrganizationRole, OrganizationRoleApi } from "@/api/models";

/**
 * What somebody is allowed to do. Not where they sit in the company — that is the org structure,
 * and the two are deliberately independent: a recruiter reports to the head of recruitment, while
 * payroll settles everybody's hours whoever they report to.
 */
export const organizationRoles: Record<OrganizationRoleApi, string> = {
	Recruiter: "Recruiter",
	HiringManager: "Hiring Manager",
	Interviewer: "Interviewer",
	Sales: "Sales",
	HumanResources: "Payroll & HR",
	Finance: "Finance",
	Administration: "Administration",
	Admin: "Admin",
};

/**
 * The read model's `OrganizationRole` carries one value the assignable `OrganizationRoleApi` does
 * not - `System`, which belongs to the platform rather than to anybody who could be given it. It
 * has no label for that reason, and falls back to its own name instead of rendering as blank.
 */
export function organizationRoleLabel(role: OrganizationRole): string {
	return (organizationRoles as Record<string, string | undefined>)[role] ?? role;
}

/**
 * Mirrors `Api/Auth/AdminPolicy.cs`. Managing people - creating them, handing out roles, setting
 * somebody's picture, signing in as them - is the administrator's job and the backend enforces it;
 * asking here only saves somebody a menu item that would answer 403.
 */
export function isAdmin(role: OrganizationRole | undefined | null): boolean {
	return role === "Admin";
}
