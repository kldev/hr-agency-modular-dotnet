import type { OrganizationRoleApi } from "@/api/models";

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
