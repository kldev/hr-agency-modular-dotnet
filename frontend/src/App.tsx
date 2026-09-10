import { createBrowserRouter, Outlet, RouterProvider } from "react-router-dom";
import { AppLayout } from "./components/layout/AppLayout";
import Stub from "./components/Stub";
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
				Component: Stub,
			},
			{
				path: ROUTES.COMPANIES,
				lazy: async () => {
					const module = await import("./features/companies/pages/CompaniesPage");

					return {
						Component: module.default,
					};
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
			},
			{
				path: ROUTES.CANDIDATES,
				Component: Stub,
			},
			{
				path: ROUTES.APPLICATIONS,
				lazy: async () => {
					const module = await import("./features/applications/pages/AplicationsPage");

					return {
						Component: module.default,
					};
				},
			},
			{
				path: ROUTES.SALES,
				Component: Stub,
			},
			{
				path: ROUTES.SALES_OPPORTUNITIES,
				Component: Stub,
			},
			{
				path: ROUTES.INTERVIEWS,
				Component: Stub,
			},
			{
				path: ROUTES.CALENDAR,
				Component: Stub,
			},
			{
				path: ROUTES.ORGANIZATIONS,
				Component: Stub,
			},
			{
				path: ROUTES.SETTINGS,
				Component: Stub,
			},
			{
				path: ROUTES.USERS,
				Component: Stub,
			},
			{
				path: ROUTES.REPORTS,
				Component: Stub,
			},
		],
	},
]);

export const App: React.FC = () => {
	return <RouterProvider router={router} />;
};
