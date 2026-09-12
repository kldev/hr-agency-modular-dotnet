export const ROUTES = {
	LOGIN: "/login",
	FORGOT_PASSWORD: "/forgot-password",
	DASHBOARD: "/",
	COMPANIES: "/companies",
	COMPANIES_DETAILS: "/companies/:id",
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

function getCandidatePath(id: string): string {
	return `${ROUTES.CANDIDATES}/${id}`;
}

export const RoutesNavigation = {
	getOpportunityPath,
	getCompanyPath,
	getCandidatePath,
};
