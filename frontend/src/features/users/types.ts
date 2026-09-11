import type { OrganizationRoleApi } from "@/api/models";

export const organizationRoles: Record<OrganizationRoleApi, string> = {
	Recruiter: "Recruiter",
	HiringManager: "Hiring Manager",
	Interviewer: "Interviewer",
	Sales: "Sales",
	Admin: "Admin",
};
