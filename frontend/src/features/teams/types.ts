import { TeamRole } from "@/api/models";

/**
 * Team roles are a division of labour and a separate vocabulary from OrganizationRole, which grants
 * access. A person can be a Recruiter on their team without being a Recruiter in the organization.
 */
export const teamRoles: Record<TeamRole, string> = {
	Lead: "Lead",
	Sales: "Sales",
	Recruiter: "Recruiter",
	Operations: "Operations",
};

export const defaultTeamRole: TeamRole = TeamRole.Recruiter;
