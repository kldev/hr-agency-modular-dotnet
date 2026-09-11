import type { RouteObject } from "react-router-dom";
import Stub from "@/components/Stub";
import NotFoundPage from "@/features/common/NotFoundPage";
import { OwnerLayout } from "@/platform-owner/layout/OwnerLayout";
import RouteFallback from "./RouteFallback";

export const OWNER_ROUTES = {
	LOGIN: "/admin",
	DASHBOARD: "/owner",
	ORGANIZATIONS: "/owner/organizations",
	USERS: "/owner/users",
	REPORTS: "/owner/reports",
	SETTINGS: "/owner/settings",
} as const;

const ownerPublicRoutes: RouteObject[] = [
	{
		path: OWNER_ROUTES.LOGIN,
		lazy: async () => {
			const module = await import("@/platform-owner/features/auth/pages/OwnerLoginPage");

			return {
				Component: module.default,
			};
		},
	},
];

const ownerRoutes: RouteObject[] = [
	...ownerPublicRoutes,
	{
		Component: OwnerLayout,
		children: [
			{
				path: OWNER_ROUTES.DASHBOARD,
				Component: Stub,
			},
			{
				path: OWNER_ROUTES.ORGANIZATIONS,
				lazy: async () => {
					const module = await import(
						"@/platform-owner/features/organizations/pages/OrganizationsPage"
					);

					return {
						Component: module.default,
					};
				},
				handle: {
					breadcrumb: "Organizations",
				},
				HydrateFallback: RouteFallback,
			},
			{
				path: OWNER_ROUTES.USERS,
				lazy: async () => {
					const module = await import("@/platform-owner/features/users/pages/UsersPage");

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
				path: OWNER_ROUTES.SETTINGS,
				Component: NotFoundPage,
			},
			{
				path: OWNER_ROUTES.REPORTS,
				Component: NotFoundPage,
			},
		],
	},
];

export { ownerPublicRoutes, ownerRoutes };
