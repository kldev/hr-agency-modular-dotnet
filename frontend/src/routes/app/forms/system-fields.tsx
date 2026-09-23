import { createFileRoute } from "@tanstack/react-router";
import SystemFieldsPage from "#/features/forms/pages/SystemFieldsPage";

export const Route = createFileRoute("/app/forms/system-fields")({
	component: SystemFieldsPage,
	staticData: { breadcrumb: "System fields" },
});
