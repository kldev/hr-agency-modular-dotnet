export const ROUTES = {
	LOGIN: "/login",
	FORGOT_PASSWORD: "/forgot-password",
	DASHBOARD: "/",
	COMPANIES: "/companies",
	JOBS: "/jobs",
	JOBS_ADD: "/jobs/add",
	CANDIDATES: "/candidates",
	APPLICATIONS: "/applications",
	CALENDAR: "calendar",
	SALES: "/sales",
	SALES_OPPORTUNITIES: "/sales/opportunities/:id",
	INTERVIEWS: "/interviews",
	ORGANIZATIONS: "/organizations",
	SETTINGS: "/settings",
	USERS: "/users",
	REPORTS: "/reports",
} as const;

export function getOpportunityPath(id: string): string {
	return `/sales/opportunities/${id}`;
}
