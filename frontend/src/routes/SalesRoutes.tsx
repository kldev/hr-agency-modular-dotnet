import type { RouteObject } from "react-router-dom";
import Stub from "@/components/Stub";
import RouteFallback from "./RouteFallback";
import { ROUTES } from "./routes";

const salesRoutes: RouteObject[] = [
	{
		path: ROUTES.SALES,
		lazy: async () => {
			const module = await import("@/features/sales/pages/SalesPage");

			return {
				Component: module.default,
			};
		},
		handle: {
			breadcrumb: "Sales",
		},
		HydrateFallback: RouteFallback,
	},
	{
		path: ROUTES.SALES_OPPORTUNITIES,
		Component: Stub,
	},
	{
		path: ROUTES.COMPANIES,
		lazy: async () => {
			const module = await import("@/features/companies/pages/CompaniesPage");

			return {
				Component: module.default,
			};
		},
		handle: {
			breadcrumb: "Companies",
		},
		HydrateFallback: RouteFallback,
	},
	{
		path: ROUTES.SALES_OPPORTUNITIES,
		Component: Stub,
	},
	{
		path: ROUTES.COMPANIES_DETAILS,
		lazy: async () => {
			const { CompanyDetailsPage } = await import("@/features/companies/pages/CompanyDetailsPage");

			return {
				Component: CompanyDetailsPage,
			};
		},
	},
];

export { salesRoutes };
