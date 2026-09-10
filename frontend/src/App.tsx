import { createBrowserRouter, RouterProvider } from "react-router-dom";
import { AppLayout } from "./components/layout/AppLayout";
import Stub from "./components/Stub";
import AccessDeniedPage from "./features/common/AccessDeniedPage";
import NotFoundPage from "./features/common/NotFoundPage";
import { ROUTES } from "./routes";

const router = createBrowserRouter([
	{
		path: ROUTES.LOGIN,
		lazy: async () => {
			const module = await import("./features/auth/pages/LoginPage");

			return {
				Component: module.default,
			};
		},
	},
	{
		path: ROUTES.FORGOT_PASSWORD,
		lazy: async () => {
			const module = await import("./features/auth/pages/ForgotPasswordPage");

			return {
				Component: module.default,
			};
		},
	},
	{
		Component: AppLayout,
		children: [
			{
				path: ROUTES.DASHBOARD,
				lazy: async () => {
					const module = await import("./features/dashboard/pages/DashboardPage");

					return {
						Component: module.default,
					};
				},
				handle: {
					breadcrumb: "Dashboard",
				},
			},
			{
				path: ROUTES.COMPANIES,
				lazy: async () => {
					const module = await import("./features/companies/pages/CompaniesPage");

					return {
						Component: module.default,
					};
				},
				handle: {
					breadcrumb: "Companies",
				},
			},
			{
				path: ROUTES.JOBS,
				lazy: async () => {
					const module = await import("./features/job-posts/pages/JobsPage");

					return {
						Component: module.default,
					};
				},
				handle: {
					breadcrumb: "Jobs",
				},
			},
			{
				path: ROUTES.CANDIDATES,
				Component: Stub,
				lazy: async () => {
					const module = await import("./features/candidates/pages/CandidatesPage");

					return {
						Component: module.default,
					};
				},
				handle: {
					breadcrumb: "Candidates",
				},
			},
			{
				path: ROUTES.APPLICATIONS,
				lazy: async () => {
					const module = await import("./features/applications/pages/AplicationsPage");

					return {
						Component: module.default,
					};
				},
				handle: {
					breadcrumb: "Applications",
				},
			},
			{
				path: ROUTES.SALES,
				lazy: async () => {
					const module = await import("./features/sales/pages/SalesPage");

					return {
						Component: module.default,
					};
				},
				handle: {
					breadcrumb: "Sales",
				},
			},
			{
				path: ROUTES.SALES_OPPORTUNITIES,
				Component: Stub,
			},
			{
				path: ROUTES.INTERVIEWS,
				lazy: async () => {
					const module = await import("./features/interviews/pages/InterviewsPage");

					return {
						Component: module.default,
					};
				},
				handle: {
					breadcrumb: "Interviews",
				},
			},
			{
				path: ROUTES.CALENDAR,
				Component: Stub,
			},
			{
				path: ROUTES.ORGANIZATIONS,
				lazy: async () => {
					const module = await import("./features/organizations/pages/OrganizationsPage");

					return {
						Component: module.default,
					};
				},
				handle: {
					breadcrumb: "Organizations",
				},
			},
			{
				path: ROUTES.SETTINGS,
				Component: AccessDeniedPage,
			},
			{
				path: ROUTES.USERS,
				lazy: async () => {
					const module = await import("./features/users/pages/UsersPage");

					return {
						Component: module.default,
					};
				},
				handle: {
					breadcrumb: "Users",
				},
			},
			{
				path: ROUTES.REPORTS,
				Component: NotFoundPage,
			},
			{
				path: "*",
				Component: NotFoundPage,
			},
		],
	},
]);

export const App: React.FC = () => {
	return <RouterProvider router={router} />;
};
