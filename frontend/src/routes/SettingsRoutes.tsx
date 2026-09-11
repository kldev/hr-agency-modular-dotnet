import type { RouteObject } from "react-router-dom";
import NotFoundPage from "@/features/common/NotFoundPage";
import RouteFallback from "./RouteFallback";
import { ROUTES } from "./routes";

const settingsRoutes: RouteObject[] = [
	{
		path: ROUTES.USERS,
		lazy: async () => {
			const module = await import("@/features/users/pages/UsersPage");

			return {
				Component: module.default,
			};
		},
		handle: {
			breadcrumb: "Users",
		},
		HydrateFallback: RouteFallback,
	},
	{
		path: ROUTES.REPORTS,
		Component: NotFoundPage,
	},
];

export { settingsRoutes };
