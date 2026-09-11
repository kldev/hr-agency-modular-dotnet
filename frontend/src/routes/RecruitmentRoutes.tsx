import type { RouteObject } from "react-router-dom";
import Stub from "@/components/Stub";
import RouteFallback from "./RouteFallback";
import { ROUTES } from "./routes";

const recruitmentRoutes: RouteObject[] = [
	{
		path: ROUTES.JOBS,
		lazy: async () => {
			const module = await import("@/features/job-posts/pages/JobsPage");

			return {
				Component: module.default,
			};
		},
		handle: {
			breadcrumb: "Jobs",
		},
		HydrateFallback: RouteFallback,
	},
	{
		path: ROUTES.CANDIDATES,
		Component: Stub,
		lazy: async () => {
			const module = await import("@/features/candidates/pages/CandidatesPage");

			return {
				Component: module.default,
			};
		},
		handle: {
			breadcrumb: "Candidates",
		},
		HydrateFallback: RouteFallback,
	},
	{
		path: ROUTES.APPLICATIONS,
		lazy: async () => {
			const module = await import("@/features/applications/pages/AplicationsPage");

			return {
				Component: module.default,
			};
		},
		handle: {
			breadcrumb: "Applications",
		},
		HydrateFallback: RouteFallback,
	},

	{
		path: ROUTES.INTERVIEWS,
		lazy: async () => {
			const module = await import("@/features/interviews/pages/InterviewsPage");

			return {
				Component: module.default,
			};
		},
		handle: {
			breadcrumb: "Interviews",
		},
		HydrateFallback: RouteFallback,
	},
	{
		path: ROUTES.CALENDAR,
		Component: Stub,
	},
];

export { recruitmentRoutes };
