export const ROUTES = {
	LOGIN: "/login",
	FORGOT_PASSWORD: "/forgot-password",
	DASHBOARD: "/",
	COMPANIES: "/companies",
	COMPANIES_DETAILS: "/companies/:id",
	JOBS: "/jobs",
	JOBS_ADD: "/jobs/add/:jdId/",
	JOBS_DETAILS: "/jobs/:id",
	JOBS_DESCRIPTION: "/job-descriptions",
	JOBS_DESCRIPTION_ADD: "/job-descriptions/add",
	CANDIDATES: "/candidates",
	APPLICATIONS: "/applications",
	APPLICATIONS_DETAILS: "/applications/:id",
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

function getApplicationPath(id: string): string {
	return `${ROUTES.APPLICATIONS}/${id}`;
}

function getJobsDetailsPath(id: string): string {
	return `${ROUTES.JOBS}/${id}`;
}

function getJobsEdit(id: string): string {
	return `${ROUTES.JOBS}/edit/${id}`;
}

function getJobsAdd(jdId: string): string {
	return `${ROUTES.JOBS}/add/${jdId}`;
}

function getJobsDescriptionDetailsPath(id: string): string {
	return `${ROUTES.JOBS_DESCRIPTION}/${id}`;
}

function getJobsDescriptionEditPath(id: string): string {
	return `${ROUTES.JOBS_DESCRIPTION}/edit/${id}`;
}

export const RoutesNavigation = {
	getOpportunityPath,
	getCompanyPath,
	getCandidatePath,
	getApplicationPath,
	getJobsDetailsPath,
	getJobsAdd,
	getJobsEdit,
	getJobsDescriptionDetailsPath,
	getJobsDescriptionEditPath,
};
