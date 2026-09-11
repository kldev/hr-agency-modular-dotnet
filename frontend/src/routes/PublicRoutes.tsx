import type { RouteObject } from "react-router-dom";
import { ROUTES } from "./routes";

const publicRoutes: RouteObject[] = [
	{
		path: ROUTES.LOGIN,
		lazy: async () => {
			const module = await import("@/features/auth/pages/LoginPage");

			return {
				Component: module.default,
			};
		},
	},
	{
		path: ROUTES.FORGOT_PASSWORD,
		lazy: async () => {
			const module = await import("@/features/auth/pages/ForgotPasswordPage");

			return {
				Component: module.default,
			};
		},
	},
];

export { publicRoutes };
