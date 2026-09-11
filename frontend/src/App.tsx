import { createBrowserRouter, RouterProvider } from "react-router-dom";
import { AppLayout } from "./components/layout/AppLayout";
import NotFoundPage from "./features/common/NotFoundPage";
import { ROUTES, RouteFallback } from "./routes";
import { ownerRoutes } from "./routes/OwnerRoutes";
import { publicRoutes } from "./routes/PublicRoutes";
import { recruitmentRoutes } from "./routes/RecruitmentRoutes";
import { salesRoutes } from "./routes/SalesRoutes";
import { settingsRoutes } from "./routes/SettingsRoutes";

const router = createBrowserRouter([
	...publicRoutes,
	{
		Component: AppLayout,
		children: [
			{
				path: ROUTES.DASHBOARD,
				lazy: async () => {
					const module = await import("@/features/dashboard/pages/DashboardPage");

					return {
						Component: module.default,
					};
				},
				handle: {
					breadcrumb: "Dashboard",
				},
				HydrateFallback: RouteFallback,
			},
			...settingsRoutes,
			...recruitmentRoutes,
			...salesRoutes,
		],
	},
	...ownerRoutes,
	{
		path: "*",
		Component: NotFoundPage,
	},
]);

export const App: React.FC = () => {
	return <RouterProvider router={router} />;
};
