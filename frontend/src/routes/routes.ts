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

function getOpportunityPath(id: string): string {
	return `${ROUTES.SALES}/opportunities/${id}`;
}

function getCompanyPath(id: string): string {
	return `${ROUTES.COMPANIES}/${id}`;
}

export const RoutesNavigation = {
	getOpportunityPath,
	getCompanyPath,
};
